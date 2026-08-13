using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class StudentProfile : BaseEntity
{
    public Guid AuthUserId { get; private set; }
    public Guid SectionId { get; private set; }
    public string? RollNumber { get; private set; }
    public DateTimeOffset? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public string? GuardianName { get; private set; }
    public string? GuardianPhone { get; private set; }
    public string? Address { get; private set; }
    public DateTimeOffset? AdmissionDate { get; private set; }

    public virtual AuthUser? AuthUser { get; private set; }
    public virtual Section? Section { get; private set; }

    private StudentProfile()
    {
    }

    public void ChangeSection(Guid sectionId)
    {
        SectionId = sectionId;
    }

    public void UpdateDetails(
        string? rollNumber,
        DateTimeOffset? dateOfBirth,
        Gender? gender,
        string? guardianName,
        string? guardianPhone,
        string? address,
        DateTimeOffset? admissionDate)
    {
        RollNumber = rollNumber;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        GuardianName = guardianName;
        GuardianPhone = guardianPhone;
        Address = address;
        AdmissionDate = admissionDate;
    }

    public static StudentProfile Create(Guid authUserId, Guid sectionId)
    {
        return new StudentProfile
        {
            AuthUserId = authUserId,
            SectionId = sectionId
        };
    }
}
