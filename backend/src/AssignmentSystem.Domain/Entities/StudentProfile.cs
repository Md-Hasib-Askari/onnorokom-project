using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities
{
    public class StudentProfile : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
    {
        public int AuthUserId { get; set; }
        public int GradeId { get; set; }
        public string? Section { get; set; }
        public string? RollNumber { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
        public string? Address { get; set; }
        public DateTimeOffset? AdmissionDate { get; set; }

        public virtual AuthUser? AuthUser { get; set; }
        public virtual Grade? Grade { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
