namespace AssignmentSystem.Application.DTOs.Assignments;

/// <summary>
/// How many submissions an assignment has, and how many of those are already graded. Returned
/// as a pair so a list screen needs one aggregate query rather than one per assignment.
/// </summary>
public record SubmissionCounts(int Total, int Graded);