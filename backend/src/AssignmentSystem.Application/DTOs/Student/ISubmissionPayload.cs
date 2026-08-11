namespace AssignmentSystem.Application.DTOs.Student;

/// <summary>
/// The shape a student sends when submitting or editing. Create and update carry the same fields
/// but stay separate records so either can gain a rule later; this contract lets one validator
/// cover both without the two rule sets drifting apart in the meantime.
/// </summary>
public interface ISubmissionPayload
{
    string? Content { get; }
    string? AttachmentUrl { get; }
}