using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IPasswordResetCodeRepository
{
    Task<PasswordResetCode?> GetLatestForUserAsync(Guid authUserId, CancellationToken ct = default);
    Task AddAsync(PasswordResetCode code, CancellationToken ct = default);
    Task UpdateAsync(PasswordResetCode code, CancellationToken ct = default);
}