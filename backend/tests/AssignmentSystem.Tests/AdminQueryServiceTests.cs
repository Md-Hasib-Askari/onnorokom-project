using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class AdminQueryServiceTests
{
    [Fact]
    public async Task GetAllAssignments_MapsAssignmentFields()
    {
        var assignment = Assignment.Create("Essay", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100);
        var repo = new FakeAssignmentRepository([assignment]);

        var sut = new AdminQueryService(repo, new FakeSubmissionRepository([]), TestMappers.CreateMapper());
        var dto = (await sut.GetAllAssignmentsAsync(new PageRequest(null), null)).Items.Single();

        Assert.Equal(assignment.Id, dto.Id);
        Assert.Equal(assignment.Title, dto.Title);
        Assert.Equal(assignment.SubjectId, dto.SubjectId);
        Assert.Equal(assignment.TeacherId, dto.TeacherId);
        Assert.Equal(assignment.Deadline, dto.Deadline);
        Assert.Equal(assignment.MaxMarks, dto.MaxMarks);
        Assert.Equal(assignment.SectionId, dto.SectionId);
        Assert.Equal(assignment.Status, dto.Status);
        Assert.Equal(0, dto.SubmissionCount);
    }

    [Fact]
    public async Task GetAllAssignments_CountsSubmissionsPerAssignment()
    {
        var counted = Assignment.Create("Counted", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100);
        var untouched = Assignment.Create("Untouched", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100);

        var submissions = new List<Submission>
        {
            Submission.Create(counted.Id, Guid.NewGuid(), content: "First"),
            Submission.Create(counted.Id, Guid.NewGuid(), content: "Second")
        };

        var sut = new AdminQueryService(
            new FakeAssignmentRepository([counted, untouched]),
            new FakeSubmissionRepository(submissions),
            TestMappers.CreateMapper());

        var dtos = (await sut.GetAllAssignmentsAsync(new PageRequest(null), null)).Items;

        Assert.Equal(2, dtos.Single(d => d.Id == counted.Id).SubmissionCount);
        Assert.Equal(0, dtos.Single(d => d.Id == untouched.Id).SubmissionCount);
    }

    [Fact]
    public async Task GetAllAssignments_CountsScopedToPageIds()
    {
        // Distinct CreatedAt values so the descending keyset order is deterministic.
        var onPage = Assignment.Create("On page", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100);
        var offPage = Assignment.Create("Off page", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100);
        ((ICreatable)onPage).CreatedAt = DateTimeOffset.UtcNow.AddHours(-1);
        ((ICreatable)offPage).CreatedAt = DateTimeOffset.UtcNow.AddHours(-2);

        var submissions = new List<Submission>
        {
            Submission.Create(onPage.Id, Guid.NewGuid(), content: "First"),
            Submission.Create(offPage.Id, Guid.NewGuid(), content: "Off-page submission")
        };

        var sut = new AdminQueryService(
            new FakeAssignmentRepository([onPage, offPage]),
            new FakeSubmissionRepository(submissions),
            TestMappers.CreateMapper());

        var page = await sut.GetAllAssignmentsAsync(new PageRequest(1), null);

        var dto = page.Items.Single();
        Assert.Equal(onPage.Id, dto.Id);
        Assert.Equal(1, dto.SubmissionCount);
    }

    [Fact]
    public async Task GetAllAssignments_SecondPage_ReturnsRemainingAndNoCursor()
    {
        var repo = new FakeAssignmentRepository(
            Enumerable.Range(1, 3)
                .Select(i => Assignment.Create($"Assignment {i}", Guid.NewGuid(), Guid.NewGuid(),
                    Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7), 100))
                .ToList());

        var sut = new AdminQueryService(repo, new FakeSubmissionRepository([]), TestMappers.CreateMapper());

        var first = await sut.GetAllAssignmentsAsync(new PageRequest(2), null);
        Assert.Equal(2, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.NotNull(first.NextCursor);

        var second = await sut.GetAllAssignmentsAsync(new PageRequest(2), first.NextCursor);
        Assert.Single(second.Items);
        Assert.False(second.HasMore);
        Assert.Null(second.NextCursor);

        Assert.Empty(first.Items.Select(a => a.Id).Intersect(second.Items.Select(a => a.Id)));
    }

    [Fact]
    public async Task GetAllSubmissions_MapsSubmissionFields()
    {
        var submission = Submission.Create(Guid.NewGuid(), Guid.NewGuid(), content: "My answer");
        var repo = new FakeSubmissionRepository([submission]);

        var sut = new AdminQueryService(new FakeAssignmentRepository([]), repo, TestMappers.CreateMapper());
        var dto = (await sut.GetAllSubmissionsAsync(new PageRequest(null), null)).Items.Single();

        Assert.Equal(submission.Id, dto.Id);
        Assert.Equal(submission.AssignmentId, dto.AssignmentId);
        Assert.Equal(submission.StudentId, dto.StudentId);
        Assert.Equal(submission.Content, dto.Content);
        Assert.Equal(submission.Status, dto.Status);
        Assert.Equal(submission.SubmittedAt, dto.SubmittedAt);
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
        {
            var ordered = assignments.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id).ToList();
            if (afterCreatedAt is not null && afterId is not null)
            {
                ordered = ordered
                    .Where(a => a.CreatedAt < afterCreatedAt || (a.CreatedAt == afterCreatedAt && a.Id < afterId))
                    .ToList();
            }

            var rows = ordered.Take(limit + 1).ToList();
            return Task.FromResult(
                PagedResult<Assignment>.FromRows(rows, limit, last => CursorCodec.Encode(last.CreatedAt, last.Id)));
        }

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
            => Task.FromResult(PagedResult<Assignment>.FromAll(
                assignments
                    .Where(a => a.SectionId == sectionId && a.Status == AssignmentStatus.Published)
                    .ToList()));

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
        {
            assignments.Remove(assignment);
            return Task.CompletedTask;
        }
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
        {
            var ordered = submissions.OrderByDescending(s => s.SubmittedAt).ThenByDescending(s => s.Id).ToList();
            if (afterSubmittedAt is not null && afterId is not null)
            {
                ordered = ordered
                    .Where(s => s.SubmittedAt < afterSubmittedAt || (s.SubmittedAt == afterSubmittedAt && s.Id < afterId))
                    .ToList();
            }

            var rows = ordered.Take(limit + 1).ToList();
            return Task.FromResult(
                PagedResult<Submission>.FromRows(rows, limit, last => CursorCodec.Encode(last.SubmittedAt, last.Id)));
        }

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
                .Max(s => s.Marks));

        public Task AddAsync(Submission submission, CancellationToken ct = default)
        {
            submissions.Add(submission);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Submission submission, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
