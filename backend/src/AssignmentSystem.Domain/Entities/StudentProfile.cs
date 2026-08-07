using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class StudentProfile : BaseEntity
{
    public Guid AuthUserId { get; private set; }
    public Guid GradeId { get; private set; }
    public string? Section { get; private set; }
    public string? RollNumber { get; private set; }
    public DateTimeOffset? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? GuardianName { get; private set; }
    public string? GuardianPhone { get; private set; }
    public string? Address { get; private set; }
    public DateTimeOffset? AdmissionDate { get; private set; }

    public virtual AuthUser? AuthUser { get; private set; }
    public virtual Grade? Grade { get; private set; }

    private StudentProfile()
    {
    }

    public void ChangeGrade(Guid gradeId)
    {
        GradeId = gradeId;
    }

    public static StudentProfile Create(Guid authUserId, Guid gradeId)
    {
        return new StudentProfile
        {
            AuthUserId = authUserId,
            GradeId = gradeId
        };
    }
}
