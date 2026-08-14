using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Admin;

/// <summary>
/// Role/status breakdown of the user base, with the admin overview's academic and assignment
/// counts. One endpoint replaces what the overview used to derive client-side from several
/// paginated lists, which could only ever cover the pages loaded so far.
/// </summary>
public sealed record AdminOverviewDto(
    int Students,
    int Teachers,
    int Admins,
    int Pending,
    int Grades,
    int Sections,
    int Subjects,
    int Assignments,
    int Drafts,
    int Published,
    int Submissions,
    int Graded,
    int Ungraded,
    IReadOnlyList<AdminRecentPendingDto> RecentPending);

/// <summary>One row of the "waiting on a decision" list on the admin overview.</summary>
public sealed record AdminRecentPendingDto(
    Guid Id,
    string FullName,
    UserRole Role,
    DateTimeOffset CreatedAt);
