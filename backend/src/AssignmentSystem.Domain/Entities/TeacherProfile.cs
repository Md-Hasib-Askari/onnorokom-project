using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class TeacherProfile : BaseEntity
{
    public Guid AuthUserId { get; private set; }
    public string? Department { get; private set; }
    public string? Designation { get; private set; }
    public string? Qualification { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public DateTimeOffset? DateOfJoining { get; private set; }

    public virtual AuthUser? AuthUser { get; private set; }

    private TeacherProfile()
    {
    }

    public static TeacherProfile Create(Guid authUserId)
    {
        return new TeacherProfile
        {
            AuthUserId = authUserId
        };
    }
}
