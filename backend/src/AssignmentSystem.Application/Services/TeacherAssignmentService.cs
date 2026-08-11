using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class TeacherAssignmentService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    ISectionSubjectRepository sectionSubjectRepository,
    ICurrentUser currentUser) : ITeacherAssignmentService
{
    public async Task<List<TeacherSectionSubjectDto>> GetMySectionSubjectsAsync(CancellationToken ct = default)
    {
        var teacherId = currentUser.GetRequiredUserId();
        var links = await sectionSubjectRepository.GetByTeacherAsync(teacherId, ct);

        return links.Select(link => new TeacherSectionSubjectDto(
            link.SectionId,
            link.Section?.Name,
            link.Section?.GradeId ?? Guid.Empty,
            link.Section?.Grade?.Name,
            link.SubjectId,
            link.Subject?.Name,
            link.Subject?.Code)).ToList();
    }

    public async Task<List<TeacherAssignmentDto>> GetMyAssignmentsAsync(CancellationToken ct = default)
    {
        var teacherId = currentUser.GetRequiredUserId();
        var assignments = await assignmentRepository.GetByTeacherAsync(teacherId, ct);

        var counts = await submissionRepository.GetCountsByAssignmentIdsAsync(assignments.Select(a => a.Id), ct);

        return assignments.Select(a => ToDto(a, counts.GetValueOrDefault(a.Id))).ToList();
    }

    public async Task<TeacherAssignmentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);
        var counts = await submissionRepository.GetCountsByAssignmentIdsAsync([id], ct);
        return ToDto(assignment, counts.GetValueOrDefault(id));
    }

    public async Task<TeacherAssignmentDto> CreateAsync(AssignmentCreateRequest request, CancellationToken ct = default)
    {
        var teacherId = currentUser.GetRequiredUserId();

        // The section-subject link is the authorization boundary: it is what an admin sets when
        // they put this teacher in front of this class for this subject.
        var teaches = await sectionSubjectRepository.ExistsForTeacherAsync(
            request.SectionId, request.SubjectId, teacherId, ct);

        if (!teaches)
        {
            throw new ForbiddenException("You are not assigned to teach this subject in this section.");
        }

        var assignment = Assignment.Create(
            request.Title,
            request.SectionId,
            request.SubjectId,
            teacherId,
            request.Deadline,
            request.MaxMarks,
            request.Description,
            request.AllowLateSubmission);

        await assignmentRepository.AddAsync(assignment, ct);

        return await GetByIdAsync(assignment.Id, ct);
    }

    public async Task<TeacherAssignmentDto> UpdateAsync(Guid id, AssignmentUpdateRequest request, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);

        var maxAwarded = await submissionRepository.GetMaxAwardedMarksAsync(id, ct);
        if (maxAwarded is { } awarded && request.MaxMarks < awarded)
        {
            throw new DomainException(
                $"Maximum marks cannot be lowered to {request.MaxMarks} because a submission has already been awarded {awarded}.");
        }

        assignment.UpdateDetails(
            request.Title,
            request.Description,
            request.Deadline,
            request.MaxMarks,
            request.AllowLateSubmission);

        await assignmentRepository.UpdateAsync(assignment, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<TeacherAssignmentDto> PublishAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);

        if (assignment.Status == AssignmentStatus.Published)
        {
            throw new DomainException("This assignment is already published.");
        }

        assignment.Publish();
        await assignmentRepository.UpdateAsync(assignment, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);

        if (await assignmentRepository.HasSubmissionsAsync(id, ct))
        {
            throw new EntityInUseException(
                "This assignment cannot be deleted because students have already submitted to it.");
        }

        await assignmentRepository.DeleteAsync(assignment, ct);
    }

    private async Task<Assignment> LoadOwnedAsync(Guid id, CancellationToken ct)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Assignment with id {id} was not found.");

        AssignmentGuards.EnsureOwnedBy(assignment, currentUser.GetRequiredUserId());
        return assignment;
    }

    private static TeacherAssignmentDto ToDto(Assignment assignment, SubmissionCounts? counts) => new(
        assignment.Id,
        assignment.Title,
        assignment.Description,
        assignment.SectionId,
        assignment.Section?.Name,
        assignment.Section?.Grade?.Name,
        assignment.SubjectId,
        assignment.Subject?.Name,
        assignment.Deadline,
        assignment.MaxMarks,
        assignment.Status,
        assignment.AllowLateSubmission,
        counts?.Total ?? 0,
        counts?.Graded ?? 0);
}