using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities
{
    public class Submission : BaseEntity, ICreatable, IUpdatable, ISoftDeletable
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string? Content { get; set; }
        public string? AttachmentUrl { get; set; }
        public SubmissionStatus Status { get; set; }
        public decimal? Marks { get; set; }
        public string? Feedback { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
        public DateTimeOffset? GradedAt { get; set; }
        public int? GradedByTeacherId { get; set; }

        public virtual Assignment? Assignment { get; set; }
        public virtual AuthUser? Student { get; set; }
        public virtual AuthUser? GradedByTeacher { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
