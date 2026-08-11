using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Submission : BaseEntity
{
    public Guid AssignmentId { get; private set; }
    public Guid StudentId { get; private set; }
    public string? Content { get; private set; }
    public string? AttachmentUrl { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public decimal? Marks { get; private set; }
    public string? Feedback { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public DateTimeOffset? GradedAt { get; private set; }
    public Guid? GradedByTeacherId { get; private set; }

    public virtual Assignment? Assignment { get; private set; }
    public virtual AuthUser? Student { get; private set; }
    public virtual AuthUser? GradedByTeacher { get; private set; }

    private Submission()
    {
    }

    public static Submission Create(Guid assignmentId, Guid studentId, string? content = null,
        string? attachmentUrl = null)
    {
        return new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            Content = content,
            AttachmentUrl = attachmentUrl,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// A student replacing their own work. There is one row per student per assignment, so the
    /// previous answer is overwritten and <see cref="SubmittedAt"/> re-stamped, which is what
    /// lateness is judged against.
    /// </summary>
    public void Revise(string? content, string? attachmentUrl)
    {
        Content = content;
        AttachmentUrl = attachmentUrl;
        Status = SubmissionStatus.Resubmitted;
        SubmittedAt = DateTimeOffset.UtcNow;
    }

    public void Grade(decimal marks, string? feedback, Guid teacherId)
    {
        Marks = marks;
        Feedback = feedback;
        Status = SubmissionStatus.Graded;
        GradedAt = DateTimeOffset.UtcNow;
        GradedByTeacherId = teacherId;
    }

    /// <summary>
    /// Hands a graded submission back for another attempt. The old mark and feedback are cleared
    /// rather than kept, so a returned submission never displays a score the student can no longer
    /// rely on. Callers pre-check <see cref="Status"/>; the throw is a last-line assertion.
    /// </summary>
    public void ReturnForRevision()
    {
        if (Status != SubmissionStatus.Graded)
        {
            throw new InvalidOperationException("Only a graded submission can be returned for revision.");
        }

        Marks = null;
        Feedback = null;
        GradedAt = null;
        GradedByTeacherId = null;
        Status = SubmissionStatus.Returned;
    }
}