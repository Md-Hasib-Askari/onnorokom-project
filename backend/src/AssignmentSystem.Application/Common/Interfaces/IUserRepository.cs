using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<List<AuthUser>> GetAllAsync(CancellationToken ct = default);
    Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountUsableAdminsAsync(CancellationToken ct = default);
    Task AddAsync(AuthUser user, CancellationToken ct = default);
    Task UpdateAsync(AuthUser user, CancellationToken ct = default);
}
