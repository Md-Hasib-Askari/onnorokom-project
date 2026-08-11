using AssignmentSystem.Application.DTOs.Student;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IStudentAssignmentService
{
    Task<List<StudentAssignmentListItemDto>> GetMyAssignmentsAsync(CancellationToken ct = default);
    Task<StudentAssignmentDetailDto> GetByIdAsync(Guid assignmentId, CancellationToken ct = default);

    Task<StudentAssignmentDetailDto> SubmitAsync(
        Guid assignmentId,
        SubmissionCreateRequest request,
        CancellationToken ct = default);

    Task<StudentAssignmentDetailDto> UpdateSubmissionAsync(
        Guid assignmentId,
        SubmissionUpdateRequest request,
        CancellationToken ct = default);
}