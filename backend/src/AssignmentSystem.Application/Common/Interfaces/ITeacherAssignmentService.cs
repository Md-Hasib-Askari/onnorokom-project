using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITeacherAssignmentService
{
    Task<PagedResult<TeacherSectionSubjectDto>> GetMySectionSubjectsAsync(CancellationToken ct = default);
    Task<PagedResult<TeacherAssignmentDto>> GetMyAssignmentsAsync(
        PageRequest page,
        string? cursor,
        CancellationToken ct = default);
    Task<TeacherAssignmentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TeacherAssignmentDto> CreateAsync(AssignmentCreateRequest request, CancellationToken ct = default);
    Task<TeacherAssignmentDto> UpdateAsync(Guid id, AssignmentUpdateRequest request, CancellationToken ct = default);
    Task<TeacherAssignmentDto> PublishAsync(Guid id, CancellationToken ct = default);
    Task<TeacherAssignmentDto> UnpublishAsync(Guid id, CancellationToken ct = default);
    Task<TeacherAssignmentDto> CloseSubmissionsAsync(Guid id, CancellationToken ct = default);
    Task<TeacherAssignmentDto> ReopenSubmissionsAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}