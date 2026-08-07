using AssignmentSystem.Application.DTOs.Assignments;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAdminQueryService
{
    Task<List<AssignmentListItemDto>> GetAllAssignmentsAsync(CancellationToken ct = default);
    Task<List<SubmissionListItemDto>> GetAllSubmissionsAsync(CancellationToken ct = default);
}
