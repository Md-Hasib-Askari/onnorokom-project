using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IGradeRepository
{
    Task<List<Grade>> GetAllAsync(CancellationToken ct = default);
    Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(string name, string academicYear, CancellationToken ct = default);
    Task<bool> HasSubjectsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasSectionsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Grade grade, CancellationToken ct = default);
    Task UpdateAsync(Grade grade, CancellationToken ct = default);
}
