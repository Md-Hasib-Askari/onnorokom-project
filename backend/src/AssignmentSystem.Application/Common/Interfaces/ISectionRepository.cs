using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISectionRepository
{
    Task<List<Section>> GetAllAsync(CancellationToken ct = default);
    Task<Section?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, SectionCounts>> GetCountsAsync(CancellationToken ct = default);
    Task<List<Section>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default);
    Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Section section, CancellationToken ct = default);
    Task UpdateAsync(Section section, CancellationToken ct = default);
}
