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
    /// The worked example: one subject the demo teacher owns, one assignment already graded and
    /// one still open, so an evaluator sees every submission state on a fresh database.
    /// </summary>
    private const string DemoSubjectName = "Mathematics";

    private const string DemoGradedAssignmentTitle = "Algebra Worksheet 1";
    private const string DemoGradedAssignmentDescription =
        "Solve questions 1 to 10 from chapter 3 and show your working for each step.";

    private const string DemoOpenAssignmentTitle = "Geometry Basics";
    private const string DemoOpenAssignmentDescription =
        "Identify the angle types in the attached figures and justify each answer in one line.";

    private const int DemoAssignmentDueInDays = 7;
    private const decimal DemoAssignmentMaxMarks = 20;

    private const string DemoSubmissionContent =
        "1. x = 4, 2. x = -3, 3. x = 7. Working attached for questions 4 to 10.";
    private const decimal DemoSubmissionMarks = 17;
    private const string DemoSubmissionFeedback =
        "Solid work overall. Question 6 lost marks for skipping the factorisation step.";

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
        await SeedDemoAssignmentsAsync(ct);
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
    /// Seeds the worked example an evaluator lands on: a subject the demo teacher is assigned to
    /// teach in the demo student's section, one published assignment carrying a graded submission,
    /// and one published assignment nobody has answered yet.
    /// </summary>
    /// <remarks>
    /// Guarded on the assignments table being empty, and deliberately past the soft-delete filter:
    /// an assignment somebody deleted while trying the system out should stay deleted rather than
    /// return on the next start. Everything is looked up by identity rather than assumed to have
    /// just been created, so an existing database that never got the example still receives it.
    /// </remarks>
    private async Task SeedDemoAssignmentsAsync(CancellationToken ct)
    {
        if (await dbContext.Assignments.IgnoreQueryFilters().AnyAsync(ct))
        {
            return;
        }

        var teacher = await dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Email == DemoTeacherEmail, ct);
        var student = await dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Email == DemoStudentEmail, ct);

        if (teacher is null || student is null)
        {
            return;
        }

        // The student's own enrolment is the section to target, rather than re-deriving the demo
        // grade: if an admin has since moved them, the example follows them instead of landing in
        // a section they can no longer see.
        var studentProfile = await dbContext.StudentProfiles
            .FirstOrDefaultAsync(p => p.AuthUserId == student.Id, ct);

        if (studentProfile is null)
        {
            return;
        }

        var section = await dbContext.Sections.FirstOrDefaultAsync(s => s.Id == studentProfile.SectionId, ct);

        if (section is null)
        {
            return;
        }

        var subject = await SeedDemoSubjectAsync(section, teacher, ct);
        await SeedDemoAssignmentPairAsync(section, subject, teacher, student, ct);
    }

    /// <summary>
    /// Returns the demo subject on the section's grade, creating it when absent, and makes sure the
    /// demo teacher holds the section-subject slot. That link is the teacher's authorization to
    /// create assignments here, so the example is unusable without it.
    /// </summary>
    private async Task<Subject> SeedDemoSubjectAsync(Section section, AuthUser teacher, CancellationToken ct)
    {
        var subject = await dbContext.Subjects
            .FirstOrDefaultAsync(s => s.GradeId == section.GradeId && s.Name == DemoSubjectName, ct);

        if (subject is null)
        {
            subject = Subject.Create(DemoSubjectName, code: null, section.GradeId);
            dbContext.Subjects.Add(subject);
            await dbContext.SaveChangesAsync(ct);
        }

        var link = await dbContext.SectionSubjects
            .FirstOrDefaultAsync(l => l.SectionId == section.Id && l.SubjectId == subject.Id, ct);

        if (link is null)
        {
            dbContext.SectionSubjects.Add(SectionSubject.Create(section.Id, subject.Id, teacher.Id));
        }
        else if (link.TeacherId is null)
        {
            link.AssignTeacher(teacher.Id);
        }

        await dbContext.SaveChangesAsync(ct);
        return subject;
    }

    private async Task SeedDemoAssignmentPairAsync(
        Section section, Subject subject, AuthUser teacher, AuthUser student, CancellationToken ct)
    {
        var deadline = DateTimeOffset.UtcNow.AddDays(DemoAssignmentDueInDays);

        var graded = Assignment.Create(
            DemoGradedAssignmentTitle, section.Id, subject.Id, teacher.Id,
            deadline, DemoAssignmentMaxMarks, DemoGradedAssignmentDescription);
        graded.Publish();

        var open = Assignment.Create(
            DemoOpenAssignmentTitle, section.Id, subject.Id, teacher.Id,
            deadline, DemoAssignmentMaxMarks, DemoOpenAssignmentDescription);
        open.Publish();

        dbContext.Assignments.AddRange(graded, open);
        await dbContext.SaveChangesAsync(ct);

        var submission = Submission.Create(graded.Id, student.Id, DemoSubmissionContent);
        submission.Grade(DemoSubmissionMarks, DemoSubmissionFeedback, teacher.Id);

        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync(ct);
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
