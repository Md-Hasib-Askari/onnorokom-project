using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class PasswordResetCodeRepository(AppDbContext dbContext) : IPasswordResetCodeRepository
{
    public async Task<PasswordResetCode?> GetLatestForUserAsync(Guid authUserId, CancellationToken ct = default)
    {
        return await dbContext.PasswordResetCodes
            .Where(c => c.AuthUserId == authUserId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public Task AddAsync(PasswordResetCode code, CancellationToken ct = default)
    {
        dbContext.PasswordResetCodes.Add(code);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PasswordResetCode code, CancellationToken ct = default)
    {
        dbContext.PasswordResetCodes.Update(code);
        return Task.CompletedTask;
    }
}