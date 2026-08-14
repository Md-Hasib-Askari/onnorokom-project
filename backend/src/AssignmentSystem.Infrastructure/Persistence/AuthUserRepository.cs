using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
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
        return await dbContext.AuthUsers.FirstOrDefaultAsync(
            u => u.RefreshToken == refreshToken || u.PreviousRefreshToken == refreshToken, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<PagedResult<AuthUser>> GetPageAsync(
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        AccountStatus? status,
        UserRole? role,
        CancellationToken ct = default)
    {
        var query = dbContext.AuthUsers.AsQueryable();
        if (status is not null)
        {
            query = query.Where(u => u.Status == status);
        }

        if (role is not null)
        {
            query = query.Where(u => u.Role == role);
        }

        var rows = await query
            .ApplyKeysetPaging(u => u.CreatedAt, afterCreatedAt, afterId, descending: false, limit)
            .ToListAsync(ct);

        return PagedResult<AuthUser>.FromRows(rows, limit, last => CursorCodec.Encode(last.CreatedAt, last.Id));
    }

    public async Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.SectionSubjects.AnyAsync(s => s.TeacherId == userId, ct);
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

    public Task<int> CountUsableAdminsAsync(CancellationToken ct = default)
    {
        return dbContext.AuthUsers.CountAsync(
            u => u.Role == UserRole.Admin && u.Status == AccountStatus.Approved && u.IsActive, ct);
    }

    public async Task<UserCounts> GetCountsAsync(CancellationToken ct = default)
    {
        var students = await dbContext.AuthUsers.CountAsync(u => u.Role == UserRole.Student, ct);
        var teachers = await dbContext.AuthUsers.CountAsync(u => u.Role == UserRole.Teacher, ct);
        var admins = await dbContext.AuthUsers.CountAsync(u => u.Role == UserRole.Admin, ct);
        var pending = await dbContext.AuthUsers.CountAsync(u => u.Status == AccountStatus.Pending, ct);
        return new UserCounts(students, teachers, admins, pending);
    }

    public async Task<List<AuthUser>> GetRecentPendingAsync(int limit, CancellationToken ct = default)
    {
        return await dbContext.AuthUsers
            .Where(u => u.Status == AccountStatus.Pending)
            .OrderByDescending(u => u.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AuthUser user, CancellationToken ct = default)
    {
        dbContext.AuthUsers.Add(user);
        await SaveAsync(user.Email, ct);
    }

    public async Task UpdateAsync(AuthUser user, CancellationToken ct = default)
    {
        dbContext.AuthUsers.Update(user);
        await SaveAsync(user.Email, ct);
    }

    private async Task SaveAsync(string email, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw new DuplicateEmailException(email);
        }
    }
}
