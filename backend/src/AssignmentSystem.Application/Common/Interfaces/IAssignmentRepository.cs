using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAssignmentRepository
{
    Task<List<Assignment>> GetAllAsync(CancellationToken ct = default);
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Assignment assignment, CancellationToken ct = default);
    Task UpdateAsync(Assignment assignment, CancellationToken ct = default);
}
