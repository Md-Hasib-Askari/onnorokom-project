using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common;

public static class UserGuards
{
    public static async Task EnsureStudentSectionValidAsync(
        ISectionRepository sectionRepository,
        UserRole role,
        Guid? studentSectionId,
        CancellationToken ct)
    {
        if (role != UserRole.Student)
        {
            return;
        }

        if (studentSectionId is null)
        {
            throw new DomainException("A section is required for student users.");
        }

        if (!await sectionRepository.ExistsAsync(studentSectionId.Value, ct))
        {
            throw new EntityNotFoundException($"Section with id {studentSectionId} was not found.");
        }
    }

    public static async Task EnsureNotLastUsableAdminAsync(
        IUserRepository userRepository,
        bool userIsUsableAdmin,
        string message,
        CancellationToken ct)
    {
        if (userIsUsableAdmin && await userRepository.CountUsableAdminsAsync(ct) <= 1)
        {
            throw new DomainException(message);
        }
    }
}
