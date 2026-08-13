using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITeacherSubmissionService
{
    Task<PagedResult<TeacherSubmissionDto>> GetForAssignmentAsync(
        Guid assignmentId,
        PageRequest page,
        string? cursor,
        CancellationToken ct = default);
    Task<TeacherSubmissionDto> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken ct = default);
    Task<TeacherSubmissionDto> ReturnAsync(Guid submissionId, CancellationToken ct = default);
}