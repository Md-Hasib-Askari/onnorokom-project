using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;

namespace AssignmentSystem.Application.Common;

public static class TeacherEligibilityGuard
{
    public static async Task EnsureIsTeacherAsync(IUserRepository userRepository, Guid teacherId, CancellationToken ct)
    {
        var teacher = await userRepository.GetByIdAsync(teacherId, ct);
        if (teacher is null || !teacher.IsUsableTeacher)
        {
            throw new InvalidTeacherException($"User with id {teacherId} is not an approved active teacher.");
        }
    }
}
