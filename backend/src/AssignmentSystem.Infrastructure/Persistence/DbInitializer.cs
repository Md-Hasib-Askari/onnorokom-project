using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class DbInitializer
{
    public const string AdminEmail = "admin@onnorokom.com";
    public const string AdminPassword = "Admin@123";

    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public DbInitializer(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _dbContext.Database.MigrateAsync(ct);

        if (await _dbContext.AuthUsers.AnyAsync(u => u.Role == UserRole.Admin, ct))
        {
            return;
        }

        var admin = new AuthUser
        {
            FullName = "System Administrator",
            Email = AdminEmail,
            PasswordHash = _passwordHasher.Hash(AdminPassword),
            Role = UserRole.Admin,
            Status = AccountStatus.Approved,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AuthUsers.Add(admin);
        await _dbContext.SaveChangesAsync(ct);
    }
}
