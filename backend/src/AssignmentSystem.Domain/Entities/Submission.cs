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
}
