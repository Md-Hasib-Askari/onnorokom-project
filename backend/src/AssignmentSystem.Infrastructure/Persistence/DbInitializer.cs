using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class DbInitializer(AppDbContext dbContext, IPasswordHasher passwordHasher)
{
    public const string AdminEmail = "admin@onnorokom.com";
    public const string AdminPassword = "Admin@123";

    public const string DemoTeacherEmail = "teacher@onnorokom.com";
    public const string DemoTeacherPassword = "Teacher@123";

    public const string DemoStudentEmail = "student@onnorokom.com";
    public const string DemoStudentPassword = "Student@123";

    private const string DefaultSectionName = "Section A";

    /// <summary>Grade the demo student is enrolled into, paired with <see cref="DefaultSectionName"/>.</summary>
    private const string DemoStudentGradeName = "Grade 1";

    /// <summary>
    /// Registration policy a fresh install starts with: teachers may sign up and wait for approval,
    /// students are created or approved by an admin. Mirrors how the system behaved before the
    /// policy was configurable, so seeding does not change an existing deployment's semantics.
    /// </summary>
    private const bool DefaultTeacherSelfRegistrationEnabled = true;
    private const bool DefaultStudentSelfRegistrationEnabled = false;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await dbContext.Database.MigrateAsync(ct);
        await SeedSystemSettingsAsync(ct);
        await SeedAdminAsync(ct);
        await SeedGradesAsync(ct);
        await SeedDemoUsersAsync(ct);
    }

    /// <summary>
    /// Inserts a row for any <see cref="SystemSettingKey"/> that has none, so adding a key later
    /// backfills on the next start instead of leaving the setting unreadable.
    /// </summary>
    private async Task SeedSystemSettingsAsync(CancellationToken ct)
    {
        var defaults = new Dictionary<SystemSettingKey, bool>
        {
            [SystemSettingKey.TeacherSelfRegistrationEnabled] = DefaultTeacherSelfRegistrationEnabled,
            [SystemSettingKey.StudentSelfRegistrationEnabled] = DefaultStudentSelfRegistrationEnabled
        };

        var existingKeys = await dbContext.SystemSettings.Select(s => s.Key).ToListAsync(ct);
        var missing = defaults
            .Where(pair => !existingKeys.Contains(pair.Key))
            .Select(pair => SystemSetting.CreateBoolean(pair.Key, pair.Value))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        dbContext.SystemSettings.AddRange(missing);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        if (await dbContext.AuthUsers.AnyAsync(u => u.Role == UserRole.Admin, ct))
        {
            return;
        }

        var admin = AuthUser.CreateApprovedAdmin("System Administrator", AdminEmail, passwordHasher.Hash(AdminPassword));

        dbContext.AuthUsers.Add(admin);
        dbContext.AdminProfiles.Add(AdminProfile.Create(admin.Id));
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task SeedGradesAsync(CancellationToken ct)
    {
        var academicYear = DateTimeOffset.UtcNow.Year.ToString();

        if (await dbContext.Grades.AnyAsync(g => g.AcademicYear == academicYear, ct))
        {
            return;
        }

        var grades = Enumerable.Range(1, 12)
            .Select(i => Grade.Create($"Grade {i}", academicYear))
            .ToList();

        // Students enrol into a section, not a grade, so a grade without one cannot take
        // students. Seed each grade a first section so a fresh database is usable as-is.
        // Only seeded alongside the grades themselves, so a section an admin later deletes
        // does not reappear on the next startup.
        var sections = grades
            .Select(grade => Section.Create(DefaultSectionName, grade.Id))
            .ToList();

        dbContext.Grades.AddRange(grades);
        dbContext.Sections.AddRange(sections);
        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Seeds one approved teacher and one approved student so every role has working demo
    /// credentials on a fresh database. Each account is seeded independently and only when its
    /// email is free, so deleting one does not block the other and never collides with an account
    /// an admin created by hand.
    /// </summary>
    private async Task SeedDemoUsersAsync(CancellationToken ct)
    {
        var teacher = await SeedDemoUserAsync(
            "Demo Teacher", DemoTeacherEmail, DemoTeacherPassword, UserRole.Teacher, ct);

        if (teacher is not null)
        {
            dbContext.TeacherProfiles.Add(TeacherProfile.Create(teacher.Id));
            await dbContext.SaveChangesAsync(ct);
        }

        // A student cannot exist without a section, so skip the demo student entirely when the
        // demo section has been deleted rather than enrolling them somewhere arbitrary. Matched
        // by name rather than by creation order, because the seeded grades all share a timestamp
        // and would otherwise hand the student a different grade on every fresh database.
        var academicYear = DateTimeOffset.UtcNow.Year.ToString();
        var section = await dbContext.Sections
            .FirstOrDefaultAsync(
                s => s.Name == DefaultSectionName
                    && s.Grade!.Name == DemoStudentGradeName
                    && s.Grade.AcademicYear == academicYear,
                ct);

        if (section is null)
        {
            return;
        }

        var student = await SeedDemoUserAsync(
            "Demo Student", DemoStudentEmail, DemoStudentPassword, UserRole.Student, ct);

        if (student is not null)
        {
            dbContext.StudentProfiles.Add(StudentProfile.Create(student.Id, section.Id));
            await dbContext.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Creates and saves an approved, active user, or returns <c>null</c> when the email is taken.
    /// </summary>
    private async Task<AuthUser?> SeedDemoUserAsync(
        string fullName, string email, string password, UserRole role, CancellationToken ct)
    {
        if (await dbContext.AuthUsers.AnyAsync(u => u.Email == email, ct))
        {
            return null;
        }

        var user = AuthUser.CreatePending(fullName, email, passwordHasher.Hash(password), role);
        user.Approve();

        dbContext.AuthUsers.Add(user);
        await dbContext.SaveChangesAsync(ct);

        return user;
    }
}
