namespace AssignmentSystem.Application.DTOs.Teacher;

/// <summary>
/// Section and subject are absent by design: they fix the assignment's audience at creation, and
/// changing them would orphan submissions already made against it.
/// </summary>
public record AssignmentUpdateRequest(
    string Title,
    string? Description,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission);