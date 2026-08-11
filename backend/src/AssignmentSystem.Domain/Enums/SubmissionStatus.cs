namespace AssignmentSystem.Domain.Enums;

/// <summary>
/// Workflow position only. Lateness is not a status: a submission can be both late and a
/// revision, so it is derived from <c>SubmittedAt</c> against the assignment deadline.
/// </summary>
public enum SubmissionStatus
{
    Submitted,
    Resubmitted,
    Returned,
    Graded
}