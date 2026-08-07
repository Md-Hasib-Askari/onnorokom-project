using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class TeacherProfile : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
{
        public Guid AuthUserId { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public string? Qualification { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTimeOffset? DateOfJoining { get; set; }

    public virtual AuthUser? AuthUser { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
