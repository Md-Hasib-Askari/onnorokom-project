using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Sections;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISectionSubjectService
{
    Task<PagedResult<SectionSubjectDto>> GetSectionSubjectsAsync(Guid sectionId, CancellationToken ct = default);
    Task<SectionSubjectDto> AssignTeacherAsync(Guid sectionId, Guid subjectId, Guid teacherId, CancellationToken ct = default);
    Task<SectionSubjectDto> UnassignTeacherAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default);
}
