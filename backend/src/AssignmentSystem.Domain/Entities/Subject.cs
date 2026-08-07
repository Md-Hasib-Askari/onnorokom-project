using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Subject : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
        public Guid GradeId { get; set; }
        public Guid? TeacherId { get; set; }

    public virtual Grade? Grade { get; set; }
    public virtual AuthUser? Teacher { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
