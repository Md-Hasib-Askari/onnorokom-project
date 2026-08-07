using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class AuthUserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public AuthUserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.AuthUsers.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await _dbContext.AuthUsers.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.AuthUsers.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<List<AuthUser>> GetByStatusAsync(AccountStatus status, CancellationToken ct = default)
    {
        return await _dbContext.AuthUsers
            .Where(u => u.Status == status)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AuthUser user, CancellationToken ct = default)
    {
        _dbContext.AuthUsers.Add(user);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AuthUser user, CancellationToken ct = default)
    {
        _dbContext.AuthUsers.Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }
}
