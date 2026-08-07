using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class AuthUserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<List<AuthUser>> GetByStatusAsync(AccountStatus status, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers
            .Where(u => u.Status == status)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<AuthUser>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.AuthUsers
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Subjects.AnyAsync(s => s.TeacherId == userId, ct);
    }

    public async Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Assignments.AnyAsync(a => a.TeacherId == userId, ct);
    }

    public async Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Submissions.AnyAsync(s => s.StudentId == userId, ct);
    }

    public async Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Submissions.AnyAsync(s => s.GradedByTeacherId == userId, ct);
    }

    public async Task AddAsync(AuthUser user, CancellationToken ct = default)
    {
        dbContext.AuthUsers.Add(user);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AuthUser user, CancellationToken ct = default)
    {
        dbContext.AuthUsers.Update(user);
        await dbContext.SaveChangesAsync(ct);
    }
}
