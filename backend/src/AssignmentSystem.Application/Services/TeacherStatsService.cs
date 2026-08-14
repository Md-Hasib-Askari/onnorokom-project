using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Application.Services;

/// <summary>
/// Counts for the teacher overview page, scoped to the signed-in teacher: assignment status
/// totals, submissions still awaiting a mark, the roster size across their taught sections, and
/// a preview of the assignments they set most recently.
/// </summary>
public class TeacherStatsService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    IProfileRepository profileRepository,
    ISectionSubjectRepository sectionSubjectRepository,
    ICurrentUser currentUser) : ITeacherStatsService
{
    private const int RecentAssignmentsLimit = 5;

    public async Task<TeacherOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var teacherId = currentUser.GetRequiredUserId();

        var assignments = await assignmentRepository.GetCountsByTeacherAsync(teacherId, ct);
        var awaitingGrading = await submissionRepository.CountUngradedForTeacherAsync(teacherId, ct);

        var links = await sectionSubjectRepository.GetByTeacherAsync(teacherId, ct);
        var sectionIds = links.Select(link => link.SectionId).Distinct().ToList();
        var students = await profileRepository.CountStudentsBySectionIdsAsync(sectionIds, ct);

        var recent = await assignmentRepository.GetRecentByTeacherAsync(teacherId, RecentAssignmentsLimit, ct);
        var counts = await submissionRepository.GetCountsByAssignmentIdsAsync(
            recent.Select(a => a.Id), ct);

        return new TeacherOverviewDto(
            assignments.Total,
            assignments.Drafts,
            assignments.Published,
            awaitingGrading,
            students,
            recent.Select(assignment =>
            {
                var count = counts.GetValueOrDefault(assignment.Id);
                return new TeacherRecentAssignmentDto(
                    assignment.Id,
                    assignment.Title,
                    assignment.Section?.Name,
                    assignment.Section?.Grade?.Name,
                    assignment.Subject?.Name,
                    assignment.Deadline,
                    assignment.Status,
                    count?.Total ?? 0,
                    count?.Graded ?? 0);
            }).ToList());
    }
}
