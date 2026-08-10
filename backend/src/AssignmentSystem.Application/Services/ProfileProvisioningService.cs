using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class ProfileProvisioningService(IProfileRepository profileRepository) : IProfileProvisioningService
{
    public Task CreateProfileAsync(AuthUser user, Guid? studentSectionId, CancellationToken ct = default)
    {
        return user.Role switch
        {
            UserRole.Teacher => profileRepository.AddAsync(TeacherProfile.Create(user.Id), ct),
            // A self-registering student has no section to enrol into yet: the admin picks one when
            // approving. The profile is created then, since StudentProfile cannot exist section-less.
            UserRole.Student when studentSectionId is null => Task.CompletedTask,
            UserRole.Student => profileRepository.AddAsync(StudentProfile.Create(user.Id, studentSectionId.Value), ct),
            UserRole.Admin => profileRepository.AddAsync(AdminProfile.Create(user.Id), ct),
            _ => Task.CompletedTask
        };
    }
}
