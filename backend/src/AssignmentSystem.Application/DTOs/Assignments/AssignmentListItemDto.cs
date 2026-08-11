using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Assignments;

/// <summary>
/// The admin's read-only row. <see cref="SubmissionCount"/> is not a property of the assignment
/// entity, so this DTO cannot be produced by mapping alone; see <c>AdminQueryService</c>.
/// </summary>
public record AssignmentListItemDto(
    Guid Id,
    string Title,
    string? Description,
    Guid SectionId,
    string? SectionName,
    Guid SubjectId,
    string? SubjectName,
    string? GradeName,
    Guid TeacherId,
    string? TeacherName,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    AssignmentStatus Status,
    bool AllowLateSubmission,
    int SubmissionCount);