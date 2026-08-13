using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class TeacherAssignmentService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    ISectionSubjectRepository sectionSubjectRepository,
    IProfileRepository profileRepository,
    ICurrentUser currentUser) : ITeacherAssignmentService
{
    public async Task<PagedResult<TeacherSectionSubjectDto>> GetMySectionSubjectsAsync(CancellationToken ct = default)
    {
        var teacherId = currentUser.GetRequiredUserId();
        var links = await sectionSubjectRepository.GetByTeacherAsync(teacherId, ct);

        var items = links.Select(link => new TeacherSectionSubjectDto(
            link.SectionId,
            link.Section?.Name,
            link.Section?.GradeId ?? Guid.Empty,
            link.Section?.Grade?.Name,
            link.SubjectId,
            link.Subject?.Name,
            link.Subject?.Code)).ToList();

        return PagedResult<TeacherSectionSubjectDto>.FromAll(items);
    }

    public async Task<PagedResult<TeacherAssignmentDto>> GetMyAssignmentsAsync(
        PageRequest page,
        string? cursor,
        CancellationToken ct = default)
    {
        var teacherId = currentUser.GetRequiredUserId();

        DateTimeOffset? afterCreatedAt = null;
        Guid? afterId = null;
        if (cursor is not null)
        {
            (afterCreatedAt, afterId) = CursorCodec.DecodeTimestamp(cursor);
        }

        var assignments = await assignmentRepository.GetPageByTeacherAsync(
            teacherId, page.Limit, afterCreatedAt, afterId, ct);

        var counts = await submissionRepository.GetCountsByAssignmentIdsAsync(
            assignments.Items.Select(a => a.Id), ct);

        return assignments.Map(a => ToDto(a, counts.GetValueOrDefault(a.Id)));
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

    public async Task<TeacherAssignmentDto> UnpublishAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);

        if (assignment.Status == AssignmentStatus.Draft)
        {
            throw new DomainException("This assignment is already a draft.");
        }

        assignment.Unpublish();
        await assignmentRepository.UpdateAsync(assignment, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<TeacherAssignmentDto> CloseSubmissionsAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);

        if (!assignment.SubmissionsOpen)
        {
            throw new DomainException("Submissions are already closed for this assignment.");
        }

        assignment.CloseSubmissions();
        await assignmentRepository.UpdateAsync(assignment, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<TeacherAssignmentDto> ReopenSubmissionsAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await LoadOwnedAsync(id, ct);

        if (assignment.SubmissionsOpen)
        {
            throw new DomainException("Submissions are already open for this assignment.");
        }

        assignment.ReopenSubmissions();
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
        assignment.SubmissionsOpen,
        counts?.Total ?? 0,
        counts?.Graded ?? 0);
}