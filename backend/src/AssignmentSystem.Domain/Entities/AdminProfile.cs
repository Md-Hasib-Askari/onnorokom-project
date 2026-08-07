using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities
{
    public class AdminProfile : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
    {
        public int AuthUserId { get; set; }
        public string? Position { get; set; }
        public string? PhoneNumber { get; set; }

        public virtual AuthUser? AuthUser { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
