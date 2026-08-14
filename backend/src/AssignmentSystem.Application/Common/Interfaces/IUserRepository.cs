using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<PagedResult<AuthUser>> GetPageAsync(
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        AccountStatus? status,
        UserRole? role,
        CancellationToken ct = default);
    Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountUsableAdminsAsync(CancellationToken ct = default);

    /// <summary>Role and status breakdown for the admin overview stats endpoint.</summary>
    Task<UserCounts> GetCountsAsync(CancellationToken ct = default);

    /// <summary>Newest pending registrations first, for the admin overview's approvals preview.</summary>
    Task<List<AuthUser>> GetRecentPendingAsync(int limit, CancellationToken ct = default);

    Task AddAsync(AuthUser user, CancellationToken ct = default);
    Task UpdateAsync(AuthUser user, CancellationToken ct = default);
}

/// <summary>Role/status totals across the user base.</summary>
public sealed record UserCounts(int Students, int Teachers, int Admins, int Pending);
