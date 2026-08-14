using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Teacher;

/// <summary>
/// The teacher overview's counts plus recent assignment previews. Replaces client-side sums over
/// paginated lists, which could only ever cover the pages loaded so far.
/// </summary>
public sealed record TeacherOverviewDto(
    int Assignments,
    int Drafts,
    int Published,
    int AwaitingGrading,
    int Students,
    IReadOnlyList<TeacherRecentAssignmentDto> RecentAssignments);

/// <summary>One row of the "recently set" list on the teacher overview.</summary>
public sealed record TeacherRecentAssignmentDto(
    Guid Id,
    string Title,
    string? SectionName,
    string? GradeName,
    string? SubjectName,
    DateTimeOffset Deadline,
    AssignmentStatus Status,
    int SubmissionCount,
    int GradedCount);
