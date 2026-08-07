using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IProfileRepository
{
    Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default);
    Task AddAsync(TeacherProfile profile, CancellationToken ct = default);
    Task AddAsync(StudentProfile profile, CancellationToken ct = default);
    Task UpdateAsync(StudentProfile profile, CancellationToken ct = default);
}
