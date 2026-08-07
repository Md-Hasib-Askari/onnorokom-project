using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Assignments;

public record SubmissionListItemDto(
    Guid Id,
    Guid AssignmentId,
    string? AssignmentTitle,
    Guid StudentId,
    string? StudentName,
    string? Content,
    string? AttachmentUrl,
    SubmissionStatus Status,
    decimal? Marks,
    string? Feedback,
    DateTimeOffset SubmittedAt);
