using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Grade : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string AcademicYear { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}