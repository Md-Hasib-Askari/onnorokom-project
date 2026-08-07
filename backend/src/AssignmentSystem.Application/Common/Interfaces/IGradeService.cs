using AssignmentSystem.Application.DTOs.Grades;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IGradeService
{
    Task<List<GradeDto>> GetAllAsync(CancellationToken ct = default);
    Task<GradeDto> CreateAsync(GradeCreateRequest request, CancellationToken ct = default);
    Task<GradeDto> UpdateAsync(Guid id, GradeUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
