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

        if (await dbContext.AuthUsers.AnyAsync(u => u.Role == UserRole.Admin, ct))
        {
            return;
        }

        var admin = AuthUser.CreateApprovedAdmin("System Administrator", AdminEmail, passwordHasher.Hash(AdminPassword));

        dbContext.AuthUsers.Add(admin);
        dbContext.AdminProfiles.Add(AdminProfile.Create(admin.Id));
        await dbContext.SaveChangesAsync(ct);
    }
}
