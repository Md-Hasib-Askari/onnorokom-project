using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Student;

/// <summary>
/// <paramref name="SubmissionStatus"/> is null when the student has not submitted at all, which the
/// UI renders as "Not submitted". <paramref name="IsLate"/> is derived from the submission time
/// against the deadline rather than stored, because a submission can be both late and a revision.
/// </summary>
public record StudentAssignmentListItemDto(
    Guid Id,
    string Title,
    string? SubjectName,
    string? TeacherName,
    DateTimeOffset Deadline,
    decimal MaxMarks,
    bool AllowLateSubmission,
    bool IsPastDeadline,
    bool SubmissionsOpen,
    SubmissionStatus? SubmissionStatus,
    bool IsLate,
    decimal? Marks);