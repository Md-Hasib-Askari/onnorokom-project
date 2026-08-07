using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Assignment : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
        public Guid SubjectId { get; set; }
        public Guid TeacherId { get; set; }
    public DateTimeOffset Deadline { get; set; }
    public decimal MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; }
    public bool AllowLateSubmission { get; set; }

    public virtual Subject? Subject { get; set; }
    public virtual AuthUser? Teacher { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
