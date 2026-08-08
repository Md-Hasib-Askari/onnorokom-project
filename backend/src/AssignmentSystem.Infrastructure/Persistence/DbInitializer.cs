using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class DbInitializer(AppDbContext dbContext, IPasswordHasher passwordHasher)
{
    public const string AdminEmail = "admin@onnorokom.com";
    public const string AdminPassword = "Admin@123";

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await dbContext.Database.MigrateAsync(ct);
        await SeedAdminAsync(ct);
        await SeedGradesAsync(ct);
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

        dbContext.Grades.AddRange(grades);
        await dbContext.SaveChangesAsync(ct);
    }
}
