using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Assignment : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid TeacherId { get; private set; }
    public DateTimeOffset Deadline { get; private set; }
    public decimal MaxMarks { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public bool AllowLateSubmission { get; private set; }

    public virtual Subject? Subject { get; private set; }
    public virtual AuthUser? Teacher { get; private set; }

    private Assignment()
    {
    }

    public static Assignment Create(
        string title,
        Guid subjectId,
        Guid teacherId,
        DateTimeOffset deadline,
        decimal maxMarks,
        string? description = null,
        bool allowLateSubmission = false)
    {
        return new Assignment
        {
            Title = title,
            SubjectId = subjectId,
            TeacherId = teacherId,
            Deadline = deadline,
            MaxMarks = maxMarks,
            Description = description,
            AllowLateSubmission = allowLateSubmission,
            Status = AssignmentStatus.Draft
        };
    }
}
