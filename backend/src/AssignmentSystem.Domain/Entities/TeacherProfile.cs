using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class TeacherProfile : BaseEntity
{
    public Guid AuthUserId { get; private set; }
    public string? TeacherCode { get; private set; }
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

    public void UpdateDetails(
        string? teacherCode,
        string? department,
        string? designation,
        string? qualification,
        string? phoneNumber,
        string? address,
        DateTimeOffset? dateOfJoining)
    {
        TeacherCode = teacherCode;
        Department = department;
        Designation = designation;
        Qualification = qualification;
        PhoneNumber = phoneNumber;
        Address = address;
        DateOfJoining = dateOfJoining;
    }

    public static TeacherProfile Create(Guid authUserId)
    {
        return new TeacherProfile
        {
            AuthUserId = authUserId
        };
    }
}
