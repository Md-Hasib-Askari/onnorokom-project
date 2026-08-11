using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Student;

/// <summary>
/// <paramref name="CanSubmit"/> and <paramref name="CanEdit"/> are computed server-side so the UI
/// never re-derives the deadline and grading rules and drifts out of step with what the API will
/// actually accept. They are mutually exclusive: submit is the first attempt, edit is every one
/// after it.
/// </summary>
public record StudentAssignmentDetailDto(
    Guid Id,
    string Title,
    string? SubjectName,
    string? TeacherName,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission,
    bool IsPastDeadline,
    SubmissionStatus? SubmissionStatus,
    bool IsLate,
    decimal? Marks,
    string? Description,
    string? Feedback,
    string? AttachmentUrl,
    string? Content,
    DateTimeOffset? SubmittedAt,
    bool CanSubmit,
    bool CanEdit);