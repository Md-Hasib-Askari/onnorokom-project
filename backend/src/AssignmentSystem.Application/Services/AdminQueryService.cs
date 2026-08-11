using AssignmentSystem.Application.Common.Interfaces;
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
    /// looks complete but is not. The counts are fetched for the whole page in one round trip.
    /// </summary>
    public async Task<List<AssignmentListItemDto>> GetAllAssignmentsAsync(CancellationToken ct = default)
    {
        var assignments = await assignmentRepository.GetAllAsync(ct);
        var counts = await submissionRepository.GetCountsByAssignmentIdsAsync(assignments.Select(a => a.Id), ct);

        return assignments
            .Select(a => ToDto(a, counts.GetValueOrDefault(a.Id)))
            .ToList();
    }

    public async Task<List<SubmissionListItemDto>> GetAllSubmissionsAsync(CancellationToken ct = default)
    {
        var submissions = await submissionRepository.GetAllAsync(ct);
        return mapper.Map<List<SubmissionListItemDto>>(submissions);
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