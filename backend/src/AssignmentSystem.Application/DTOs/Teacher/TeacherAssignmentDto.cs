using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Teacher;

public record TeacherAssignmentDto(
    Guid Id,
    string Title,
    string? Description,
    Guid SectionId,
    string? SectionName,
    string? GradeName,
    Guid SubjectId,
    string? SubjectName,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    AssignmentStatus Status,
    bool AllowLateSubmission,
    int SubmissionCount,
    int GradedCount);