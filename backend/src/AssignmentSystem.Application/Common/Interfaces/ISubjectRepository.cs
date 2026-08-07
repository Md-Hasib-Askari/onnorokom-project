using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync(CancellationToken ct = default);
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, Guid gradeId, CancellationToken ct = default);
    Task<bool> HasAssignmentsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Subject subject, CancellationToken ct = default);
    Task UpdateAsync(Subject subject, CancellationToken ct = default);
}
