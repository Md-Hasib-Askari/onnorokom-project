using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Admin;

namespace AssignmentSystem.Application.Services;

/// <summary>
/// Counts for the admin overview page. The academic catalog (grades, sections, subjects) is small
/// enough to load in full; the rest comes from dedicated count queries, so the figures are real
/// totals rather than sums over the first page of a paginated list.
/// </summary>
public class AdminStatsService(
    IUserRepository userRepository,
    IGradeRepository gradeRepository,
    ISectionRepository sectionRepository,
    ISubjectRepository subjectRepository,
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository) : IAdminStatsService
{
    private const int RecentPendingLimit = 5;

    public async Task<AdminOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetCountsAsync(ct);
        var grades = await gradeRepository.GetAllAsync(ct);
        var sections = await sectionRepository.GetAllAsync(ct);
        var subjects = await subjectRepository.GetAllAsync(ct);
        var assignments = await assignmentRepository.GetCountsAsync(ct);
        var submissions = await submissionRepository.GetCountsAsync(ct);
        var recentPending = await userRepository.GetRecentPendingAsync(RecentPendingLimit, ct);

        return new AdminOverviewDto(
            users.Students,
            users.Teachers,
            users.Admins,
            users.Pending,
            grades.Count,
            sections.Count,
            subjects.Count,
            assignments.Total,
            assignments.Drafts,
            assignments.Published,
            submissions.Total,
            submissions.Graded,
            submissions.Total - submissions.Graded,
            recentPending.Select(user => new AdminRecentPendingDto(
                user.Id,
                user.FullName,
                user.Role,
                user.CreatedAt)).ToList());
    }
}
