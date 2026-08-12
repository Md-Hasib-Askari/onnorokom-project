using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAdminQueryService
{
    Task<PagedResult<AssignmentListItemDto>> GetAllAssignmentsAsync(
        PageRequest page,
        string? cursor,
        CancellationToken ct = default);
    Task<PagedResult<SubmissionListItemDto>> GetAllSubmissionsAsync(
        PageRequest page,
        string? cursor,
        CancellationToken ct = default);
}
