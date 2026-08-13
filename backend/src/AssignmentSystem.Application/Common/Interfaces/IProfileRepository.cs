using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IProfileRepository
{
    Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default);
    Task<List<StudentProfile>> GetStudentsByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default);

    /// <summary>Roster size across a set of sections, for the teacher overview stats endpoint.</summary>
    Task<int> CountStudentsBySectionIdsAsync(IEnumerable<Guid> sectionIds, CancellationToken ct = default);

    /// <summary>
    /// Keyset page of students across a set of sections ordered by
    /// <c>(Section.Name, FullName, Id)</c> ascending, which keeps the roster grouped by class.
    /// </summary>
    Task<PagedResult<StudentProfile>> GetStudentsPageBySectionIdsAsync(
        IEnumerable<Guid> sectionIds,
        int limit,
        string? afterSectionName,
        string? afterFullName,
        Guid? afterId,
        CancellationToken ct = default);
    Task<TeacherProfile?> GetTeacherByUserIdAsync(Guid authUserId, CancellationToken ct = default);
    Task<List<TeacherProfile>> GetTeachersByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default);
    Task<AdminProfile?> GetAdminByUserIdAsync(Guid authUserId, CancellationToken ct = default);
    Task AddAsync(TeacherProfile profile, CancellationToken ct = default);
    Task AddAsync(StudentProfile profile, CancellationToken ct = default);
    Task AddAsync(AdminProfile profile, CancellationToken ct = default);
    Task UpdateAsync(StudentProfile profile, CancellationToken ct = default);
    Task UpdateAsync(TeacherProfile profile, CancellationToken ct = default);
    Task UpdateAsync(AdminProfile profile, CancellationToken ct = default);
    Task SoftDeleteForUserAsync(Guid authUserId, CancellationToken ct = default);
}
