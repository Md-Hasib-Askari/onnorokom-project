using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Teacher;

/// <summary>
/// <paramref name="IsLate"/> is derived from the submission time against the assignment deadline
/// rather than stored, because a submission can be both late and a revision and
/// <see cref="SubmissionStatus"/> only tracks one of those.
/// </summary>
public record TeacherSubmissionDto(
    Guid Id,
    Guid StudentId,
    string? StudentName,
    string? RollNumber,
    string? Content,
    string? AttachmentUrl,
    SubmissionStatus Status,
    bool IsLate,
    decimal? Marks,
    string? Feedback,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? GradedAt);