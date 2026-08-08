using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common;

public static class UserGuards
{
    public static async Task EnsureStudentGradeValidAsync(
        IGradeRepository gradeRepository,
        UserRole role,
        Guid? studentGradeId,
        CancellationToken ct)
    {
        if (role != UserRole.Student)
        {
            return;
        }

        if (studentGradeId is null)
        {
            throw new DomainException("A grade is required for student users.");
        }

        if (!await gradeRepository.ExistsAsync(studentGradeId.Value, ct))
        {
            throw new EntityNotFoundException($"Grade with id {studentGradeId} was not found.");
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
