using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Assignments;

public record AssignmentListItemDto(
    Guid Id,
    string Title,
    string? Description,
    Guid SubjectId,
    string? SubjectName,
    string? GradeName,
    Guid TeacherId,
    string? TeacherName,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    AssignmentStatus Status,
    bool AllowLateSubmission);
