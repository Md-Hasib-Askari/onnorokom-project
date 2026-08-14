using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Profile;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common;

/// <summary>
/// Builds the three profile detail blocks for a single user in one round trip per profile type.
/// The profile DTOs are assembled by hand because each one needs navigations (section, grade) that
/// a mapper cannot fetch, and the detail reads (profile page, admin user detail) share the shape.
/// </summary>
public static class ProfileDetailDtoFactory
{
    public static async Task<(StudentProfileDetailDto?, TeacherProfileDetailDto?, AdminProfileDetailDto?)> BuildAsync(
        AuthUser user,
        IProfileRepository profileRepository,
        ISectionRepository sectionRepository,
        CancellationToken ct = default)
    {
        StudentProfileDetailDto? studentProfile = null;
        TeacherProfileDetailDto? teacherProfile = null;
        AdminProfileDetailDto? adminProfile = null;

        if (user.Role == UserRole.Student)
        {
            var profile = await profileRepository.GetStudentByUserIdAsync(user.Id, ct);
            if (profile is not null)
            {
                var section = profile.SectionId == Guid.Empty
                    ? null
                    : await sectionRepository.GetByIdAsync(profile.SectionId, ct);

                studentProfile = new StudentProfileDetailDto(
                    profile.SectionId,
                    section?.Name,
                    section?.Grade?.Name,
                    profile.RollNumber,
                    profile.DateOfBirth,
                    profile.Gender,
                    profile.GuardianName,
                    profile.GuardianPhone,
                    profile.Address,
                    profile.AdmissionDate);
            }
        }
        else if (user.Role == UserRole.Teacher)
        {
            var profile = await profileRepository.GetTeacherByUserIdAsync(user.Id, ct);
            if (profile is not null)
            {
                teacherProfile = new TeacherProfileDetailDto(
                    profile.TeacherCode,
                    profile.Department,
                    profile.Designation,
                    profile.Qualification,
                    profile.PhoneNumber,
                    profile.Address,
                    profile.DateOfJoining);
            }
        }
        else if (user.Role == UserRole.Admin)
        {
            var profile = await profileRepository.GetAdminByUserIdAsync(user.Id, ct);
            if (profile is not null)
            {
                adminProfile = new AdminProfileDetailDto(
                    profile.Position,
                    profile.PhoneNumber);
            }
        }

        return (studentProfile, teacherProfile, adminProfile);
    }
}
