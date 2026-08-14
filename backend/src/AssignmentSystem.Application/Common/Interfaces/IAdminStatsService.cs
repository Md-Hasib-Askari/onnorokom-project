using AssignmentSystem.Application.DTOs.Admin;

namespace AssignmentSystem.Application.Common.Interfaces;

/// <summary>Counts backing the admin overview page.</summary>
public interface IAdminStatsService
{
    Task<AdminOverviewDto> GetOverviewAsync(CancellationToken ct = default);
}
