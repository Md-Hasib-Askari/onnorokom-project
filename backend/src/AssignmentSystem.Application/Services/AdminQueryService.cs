using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class AdminQueryService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    IMapper mapper) : IAdminQueryService
{
    /// <summary>
    /// Built by hand rather than through AutoMapper because the submission count comes from a
    /// second query, and a mapper that emitted a placeholder for it would hand callers a DTO that
    /// looks complete but is not. The counts are fetched for the page's ids in one round trip.
    /// </summary>
    public async Task<PagedResult<AssignmentListItemDto>> GetAllAssignmentsAsync(
        PageRequest page,
        string? cursor,
        CancellationToken ct = default)
    {
        var (afterCreatedAt, afterId) = cursor is null
            ? (afterCreatedAt: (DateTimeOffset?)null, afterId: (Guid?)null)
            : CursorCodec.DecodeTimestamp(cursor);

        var assignments = await assignmentRepository.GetPageAsync(page.Limit, afterCreatedAt, afterId, ct);
        var counts = await submissionRepository.GetCountsByAssignmentIdsAsync(assignments.Items.Select(a => a.Id), ct);

        var items = assignments.Items
            .Select(a => ToDto(a, counts.GetValueOrDefault(a.Id)))
            .ToList();

        return new PagedResult<AssignmentListItemDto>(items, assignments.NextCursor, assignments.HasMore);
    }

    public async Task<PagedResult<SubmissionListItemDto>> GetAllSubmissionsAsync(
        PageRequest page,
        string? cursor,
        CancellationToken ct = default)
    {
        var (afterSubmittedAt, afterId) = cursor is null
            ? (afterSubmittedAt: (DateTimeOffset?)null, afterId: (Guid?)null)
            : CursorCodec.DecodeTimestamp(cursor);

        var submissions = await submissionRepository.GetPageAsync(page.Limit, afterSubmittedAt, afterId, ct);
        var items = mapper.Map<List<SubmissionListItemDto>>(submissions.Items);
        return new PagedResult<SubmissionListItemDto>(items, submissions.NextCursor, submissions.HasMore);
    }

    /// <summary>
    /// Grade is read from the subject rather than the section: the two always agree, because a
    /// teacher can only target a section-subject pair an admin linked, and a subject belongs to
    /// exactly one grade.
    /// </summary>
    private static AssignmentListItemDto ToDto(Assignment assignment, SubmissionCounts? counts) => new(
        assignment.Id,
        assignment.Title,
        assignment.Description,
        assignment.SectionId,
        assignment.Section?.Name,
        assignment.SubjectId,
        assignment.Subject?.Name,
        assignment.Subject?.Grade?.Name,
        assignment.TeacherId,
        assignment.Teacher?.FullName,
        assignment.Deadline,
        assignment.MaxMarks,
        assignment.Status,
        assignment.AllowLateSubmission,
        counts?.Total ?? 0);
}
