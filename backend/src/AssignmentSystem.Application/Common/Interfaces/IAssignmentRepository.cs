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
    /// Keyset page of the student-facing feed ordered by <c>(CreatedAt, Id)</c> descending.
    /// Drafts are excluded at the query rather than filtered later, so a caller cannot
    /// accidentally surface unpublished work by forgetting a check.
    /// </summary>
    Task<PagedResult<Assignment>> GetPublishedPageForSectionAsync(
        Guid sectionId,
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        CancellationToken ct = default);

    Task<bool> HasSubmissionsAsync(Guid assignmentId, CancellationToken ct = default);

    /// <summary>Assignment totals by status for the admin overview stats endpoint.</summary>
    Task<AssignmentCounts> GetCountsAsync(CancellationToken ct = default);

    /// <summary>One teacher's assignment totals by status for the teacher overview stats endpoint.</summary>

    Task AddAsync(Assignment assignment, CancellationToken ct = default);
    Task UpdateAsync(Assignment assignment, CancellationToken ct = default);
    Task DeleteAsync(Assignment assignment, CancellationToken ct = default);
}

/// <summary>Assignment totals by status.</summary>
public sealed record AssignmentCounts(int Total, int Drafts, int Published);