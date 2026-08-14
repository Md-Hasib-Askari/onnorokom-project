using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Subjects;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISubjectService
{
    Task<PagedResult<SubjectDto>> GetAllAsync(CancellationToken ct = default);
    Task<SubjectDto> CreateAsync(SubjectCreateRequest request, CancellationToken ct = default);
    Task<SubjectDto> UpdateAsync(Guid id, SubjectUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
