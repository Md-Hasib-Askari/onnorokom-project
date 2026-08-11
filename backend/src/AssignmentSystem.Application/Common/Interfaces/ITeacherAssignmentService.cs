using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITeacherAssignmentService
{
    Task<List<TeacherSectionSubjectDto>> GetMySectionSubjectsAsync(CancellationToken ct = default);
    Task<List<TeacherAssignmentDto>> GetMyAssignmentsAsync(CancellationToken ct = default);
    Task<TeacherAssignmentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TeacherAssignmentDto> CreateAsync(AssignmentCreateRequest request, CancellationToken ct = default);
    Task<TeacherAssignmentDto> UpdateAsync(Guid id, AssignmentUpdateRequest request, CancellationToken ct = default);
    Task<TeacherAssignmentDto> PublishAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}