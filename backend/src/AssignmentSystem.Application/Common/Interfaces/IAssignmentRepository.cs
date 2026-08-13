using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAssignmentRepository
{
    /// <summary>Keyset page ordered by <c>(CreatedAt, Id)</c> descending (newest first).</summary>
    Task<PagedResult<Assignment>> GetPageAsync(
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        CancellationToken ct = default);
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Keyset page of one teacher's assignments ordered by <c>(CreatedAt, Id)</c> descending.</summary>
    Task<PagedResult<Assignment>> GetPageByTeacherAsync(
        Guid teacherId,
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        CancellationToken ct = default);

    /// <summary>
    /// The student-facing feed. Drafts are excluded at the query rather than filtered later, so a
    /// caller cannot accidentally surface unpublished work by forgetting a check.
    /// </summary>
    Task<List<Assignment>> GetPublishedForSectionAsync(Guid sectionId, CancellationToken ct = default);

    Task<bool> HasSubmissionsAsync(Guid assignmentId, CancellationToken ct = default);
    Task AddAsync(Assignment assignment, CancellationToken ct = default);
    Task UpdateAsync(Assignment assignment, CancellationToken ct = default);
    Task DeleteAsync(Assignment assignment, CancellationToken ct = default);
}