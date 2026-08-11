using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Assignment : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid SectionId { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid TeacherId { get; private set; }
    public DateTimeOffset Deadline { get; private set; }
    public decimal MaxMarks { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public bool AllowLateSubmission { get; private set; }

    public virtual Section? Section { get; private set; }
    public virtual Subject? Subject { get; private set; }
    public virtual AuthUser? Teacher { get; private set; }

    private Assignment()
    {
    }

    public static Assignment Create(
        string title,
        Guid sectionId,
        Guid subjectId,
        Guid teacherId,
        DateTimeOffset deadline,
        decimal maxMarks,
        string? description = null,
        bool allowLateSubmission = false)
    {
        return new Assignment
        {
            Title = title.Trim(),
            SectionId = sectionId,
            SubjectId = subjectId,
            TeacherId = teacherId,
            Deadline = deadline,
            MaxMarks = maxMarks,
            Description = description,
            AllowLateSubmission = allowLateSubmission,
            Status = AssignmentStatus.Draft
        };
    }

    /// <summary>
    /// Section, subject and author are fixed at creation: changing them would move the assignment
    /// to a different audience, invalidating any submissions already attached to it.
    /// </summary>
    public void UpdateDetails(
        string title,
        string? description,
        DateTimeOffset deadline,
        decimal maxMarks,
        bool allowLateSubmission)
    {
        Title = title.Trim();
        Description = description;
        Deadline = deadline;
        MaxMarks = maxMarks;
        AllowLateSubmission = allowLateSubmission;
    }

    /// <summary>
    /// Publishing is one-way. Callers pre-check <see cref="Status"/> and surface a domain error;
    /// the throw here is a last-line assertion so a missed check cannot silently no-op.
    /// </summary>
    public void Publish()
    {
        if (Status == AssignmentStatus.Published)
        {
            throw new InvalidOperationException("Assignment is already published.");
        }

        Status = AssignmentStatus.Published;
    }

    public bool IsPastDeadline(DateTimeOffset now) => now > Deadline;

    public bool IsAcceptingSubmissions(DateTimeOffset now) =>
        Status == AssignmentStatus.Published && (!IsPastDeadline(now) || AllowLateSubmission);
}