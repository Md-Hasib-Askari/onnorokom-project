using AssignmentSystem.Application.DTOs.Sections;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISectionService
{
    Task<List<SectionDto>> GetAllAsync(CancellationToken ct = default);
    Task<SectionDto> CreateAsync(SectionCreateRequest request, CancellationToken ct = default);
    Task<SectionDto> UpdateAsync(Guid id, SectionUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
