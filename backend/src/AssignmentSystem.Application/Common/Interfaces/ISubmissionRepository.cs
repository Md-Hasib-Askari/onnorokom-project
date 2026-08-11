using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISubmissionRepository
{
    Task<List<Submission>> GetAllAsync(CancellationToken ct = default);
    Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
    Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default);
    Task<Dictionary<Guid, SubmissionCounts>> GetCountsByAssignmentIdsAsync(IEnumerable<Guid> assignmentIds, CancellationToken ct = default);

    /// <summary>
    /// The highest mark already awarded on an assignment, or null when nothing is graded yet.
    /// Lowering <c>MaxMarks</c> under this would leave a student scoring above the maximum.
    /// </summary>
    Task<decimal?> GetMaxAwardedMarksAsync(Guid assignmentId, CancellationToken ct = default);

    Task AddAsync(Submission submission, CancellationToken ct = default);
    Task UpdateAsync(Submission submission, CancellationToken ct = default);
}