using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITeacherSubmissionService
{
    Task<List<TeacherSubmissionDto>> GetForAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
    Task<TeacherSubmissionDto> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken ct = default);
    Task<TeacherSubmissionDto> ReturnAsync(Guid submissionId, CancellationToken ct = default);
}