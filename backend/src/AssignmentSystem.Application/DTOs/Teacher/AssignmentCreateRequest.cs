namespace AssignmentSystem.Application.DTOs.Teacher;

public record AssignmentCreateRequest(
    string Title,
    string? Description,
    Guid SectionId,
    Guid SubjectId,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission);