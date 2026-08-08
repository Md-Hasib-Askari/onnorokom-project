using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class ProfileProvisioningService(IProfileRepository profileRepository) : IProfileProvisioningService
{
    public Task CreateProfileAsync(AuthUser user, Guid? studentGradeId, CancellationToken ct = default)
    {
        return user.Role switch
        {
            UserRole.Teacher => profileRepository.AddAsync(TeacherProfile.Create(user.Id), ct),
            UserRole.Student => profileRepository.AddAsync(StudentProfile.Create(user.Id, studentGradeId!.Value), ct),
            UserRole.Admin => profileRepository.AddAsync(AdminProfile.Create(user.Id), ct),
            _ => Task.CompletedTask
        };
    }
}
