using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class TeacherStatsServiceTests
{
    [Fact]
    public async Task GetOverview_ReturnsTeacherScopedCounts()
    {
        var teacherId = Guid.NewGuid();
        var sectionA = Section.Create("Section A", Guid.NewGuid());
        var sectionB = Section.Create("Section B", Guid.NewGuid());

        var draft = Assignment.Create("Draft", sectionA.Id, Guid.NewGuid(), teacherId,
            DateTimeOffset.UtcNow.AddDays(7), 100);
        var published = Assignment.Create("Published", sectionA.Id, Guid.NewGuid(), teacherId,
            DateTimeOffset.UtcNow.AddDays(7), 100);
        published.Publish();
        var otherTeacherAssignment = Assignment.Create("Not mine", sectionA.Id, Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100);

        var studentOne = AuthUser.CreatePending("Student One", "one@example.com", "hash", UserRole.Student);
        var studentTwo = AuthUser.CreatePending("Student Two", "two@example.com", "hash", UserRole.Student);
        var ungraded = Submission.Create(published.Id, studentOne.Id, content: "Pending mark");
        var graded = Submission.Create(published.Id, studentTwo.Id, content: "Graded");
        graded.Grade(18, "Nice work", teacherId);

        var sut = new TeacherStatsService(
            new FakeAssignmentRepository([draft, published, otherTeacherAssignment]),
            new FakeSubmissionRepository([ungraded, graded]),
            new FakeProfileRepository(),
            new FakeSectionSubjectRepository([sectionA.Id, sectionA.Id, sectionB.Id]),
            new FakeCurrentUser(teacherId));

        var overview = await sut.GetOverviewAsync();

        Assert.Equal(2, overview.Assignments);
        Assert.Equal(1, overview.Drafts);
        Assert.Equal(1, overview.Published);
        Assert.Equal(1, overview.AwaitingGrading);
        Assert.Equal(2, overview.Students);
        Assert.Equal(2, overview.RecentAssignments.Count);
        var publishedPreview = overview.RecentAssignments.Single(a => a.Id == published.Id);
        Assert.Equal(2, publishedPreview.SubmissionCount);
        Assert.Equal(1, publishedPreview.GradedCount);
        var draftPreview = overview.RecentAssignments.Single(a => a.Id == draft.Id);
        Assert.Equal(0, draftPreview.SubmissionCount);
        Assert.Equal(0, draftPreview.GradedCount);
    }

    [Fact]
    public async Task GetOverview_TeacherWithNoAssignments_ReturnsEmptyPreview()
    {
        var sut = new TeacherStatsService(
            new FakeAssignmentRepository([]),
            new FakeSubmissionRepository([]),
            new FakeProfileRepository(),
            new FakeSectionSubjectRepository([]),
            new FakeCurrentUser(Guid.NewGuid()));

        var overview = await sut.GetOverviewAsync();

        Assert.Equal(0, overview.Assignments);
        Assert.Equal(0, overview.AwaitingGrading);
        Assert.Equal(0, overview.Students);
        Assert.Empty(overview.RecentAssignments);
    }

    private sealed class FakeAssignmentRepository(List<Assignment> assignments) : IAssignmentRepository
    {
        public Task<AssignmentCounts> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(CountAssignments(null));

        public Task<AssignmentCounts> GetCountsByTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(CountAssignments(teacherId));

        public Task<List<Assignment>> GetRecentByTeacherAsync(Guid teacherId, int limit, CancellationToken ct = default)
            => Task.FromResult(assignments
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList());

        private AssignmentCounts CountAssignments(Guid? teacherId)
        {
            var source = teacherId is null ? assignments : assignments.Where(a => a.TeacherId == teacherId);
            var drafts = source.Count(a => a.Status == AssignmentStatus.Draft);
            var published = source.Count(a => a.Status == AssignmentStatus.Published);
            return new AssignmentCounts(drafts + published, drafts, published);
        }

        public Task<PagedResult<Assignment>> GetPageAsync(
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(assignments));

        public Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(assignments.FirstOrDefault(a => a.Id == id));

        public Task<PagedResult<Assignment>> GetPageByTeacherAsync(
            Guid teacherId,
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(
                assignments.Where(a => a.TeacherId == teacherId).ToList()));

        public Task<PagedResult<Assignment>> GetPublishedPageForSectionAsync(
            Guid sectionId,
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(assignments));

        public Task<bool> HasSubmissionsAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Assignment assignment, CancellationToken ct = default)
        {
            assignments.Add(assignment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Assignment assignment, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Assignment assignment, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSubmissionRepository(List<Submission> submissions) : ISubmissionRepository
    {
        public Task<SubmissionCounts> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(new SubmissionCounts(
                submissions.Count,
                submissions.Count(s => s.Status == SubmissionStatus.Graded)));

        public Task<int> CountUngradedForTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(submissions.Count(s => s.Status != SubmissionStatus.Graded));

        public Task<PagedResult<Submission>> GetPageAsync(
            int limit,
            DateTimeOffset? afterSubmittedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Submission>.FromAll(submissions));

        public Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(submissions.FirstOrDefault(s => s.Id == id));

        public Task<PagedResult<Submission>> GetPageByAssignmentAsync(
            Guid assignmentId,
            int limit,
            string? afterFullName,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Submission>.FromAll(
                submissions.Where(s => s.AssignmentId == assignmentId).ToList()));

        public Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default)
            => Task.FromResult(submissions.FirstOrDefault(s => s.AssignmentId == assignmentId && s.StudentId == studentId));

        public Task<List<Submission>> GetByStudentAndAssignmentIdsAsync(
            Guid studentId,
            IEnumerable<Guid> assignmentIds,
            CancellationToken ct = default)
        {
            var ids = assignmentIds.ToHashSet();
            return Task.FromResult(submissions
                .Where(s => s.StudentId == studentId && ids.Contains(s.AssignmentId))
                .ToList());
        }

        public Task<Dictionary<Guid, SubmissionCounts>> GetCountsByAssignmentIdsAsync(
            IEnumerable<Guid> assignmentIds,
            CancellationToken ct = default)
        {
            var ids = assignmentIds.ToHashSet();
            return Task.FromResult(submissions
                .Where(s => ids.Contains(s.AssignmentId))
                .GroupBy(s => s.AssignmentId)
                .ToDictionary(
                    g => g.Key,
                    g => new SubmissionCounts(g.Count(), g.Count(s => s.Status == SubmissionStatus.Graded))));
        }

        public Task<decimal?> GetMaxAwardedMarksAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(submissions
                .Where(s => s.AssignmentId == assignmentId && s.Marks != null)
                .Select(s => s.Marks)
                .Max());

        public Task AddAsync(Submission submission, CancellationToken ct = default)
        {
            submissions.Add(submission);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Submission submission, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public Task<int> CountStudentsBySectionIdsAsync(IEnumerable<Guid> sectionIds, CancellationToken ct = default)
            => Task.FromResult(sectionIds.Count());

        public Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult<StudentProfile?>(null);

        public Task<List<StudentProfile>> GetStudentsByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
            => Task.FromResult(new List<StudentProfile>());

        public Task<PagedResult<StudentProfile>> GetStudentsPageBySectionIdsAsync(
            IEnumerable<Guid> sectionIds,
            int limit,
            string? afterSectionName,
            string? afterFullName,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<StudentProfile>.FromAll(new List<StudentProfile>()));

        public Task<TeacherProfile?> GetTeacherByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult<TeacherProfile?>(null);

        public Task<List<TeacherProfile>> GetTeachersByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
            => Task.FromResult(new List<TeacherProfile>());

        public Task<AdminProfile?> GetAdminByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult<AdminProfile?>(null);

        public Task AddAsync(TeacherProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task AddAsync(StudentProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task AddAsync(AdminProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(StudentProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(TeacherProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(AdminProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SoftDeleteForUserAsync(Guid authUserId, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSectionSubjectRepository(List<Guid> sectionIds) : ISectionSubjectRepository
    {
        public Task<List<SectionSubject>> GetBySectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.FromResult(new List<SectionSubject>());

        public Task<SectionSubject?> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default)
            => Task.FromResult<SectionSubject?>(null);

        public Task<List<SectionSubject>> GetByTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(sectionIds.Select(id => SectionSubject.Create(id, Guid.NewGuid(), teacherId)).ToList());

        public Task<bool> ExistsForTeacherAsync(Guid sectionId, Guid subjectId, Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(SectionSubject link, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(SectionSubject link, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SoftDeleteForSectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SoftDeleteForSubjectAsync(Guid subjectId, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUser
    {
        public string? UserId => userId.ToString();
    }
}
