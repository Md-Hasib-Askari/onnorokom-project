using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class TeacherSubmissionServiceTests
{
    private readonly FakeSubmissionRepository _submissions = new();
    private readonly FakeAssignmentRepository _assignments = new();
    private readonly FakeProfileRepository _profiles = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly TeacherSubmissionService _sut;

    private readonly Guid _teacherId = Guid.NewGuid();
    private readonly Guid _studentId = Guid.NewGuid();

    public TeacherSubmissionServiceTests()
    {
        _currentUser.UserId = _teacherId.ToString();
        _sut = new TeacherSubmissionService(_submissions, _assignments, _profiles, _currentUser);
    }

    private Assignment SeedAssignment(Guid? teacherId = null, decimal maxMarks = 100, DateTimeOffset? deadline = null)
    {
        var assignment = Assignment.Create("Essay", Guid.NewGuid(), Guid.NewGuid(), teacherId ?? _teacherId,
            deadline ?? DateTimeOffset.UtcNow.AddDays(7), maxMarks);
        _assignments.Items.Add(assignment);
        return assignment;
    }

    private Submission SeedSubmission(Assignment assignment)
    {
        var submission = Submission.Create(assignment.Id, _studentId, "My answer");
        _submissions.Items.Add(submission);
        return submission;
    }

    [Fact]
    public async Task Grade_SetsGradedStatusMarksAndGrader()
    {
        var assignment = SeedAssignment();
        var submission = SeedSubmission(assignment);

        var dto = await _sut.GradeAsync(submission.Id, new GradeSubmissionRequest(85, "Well argued."));

        Assert.Equal(SubmissionStatus.Graded, submission.Status);
        Assert.Equal(85m, submission.Marks);
        Assert.Equal("Well argued.", submission.Feedback);
        Assert.Equal(_teacherId, submission.GradedByTeacherId);
        Assert.NotNull(submission.GradedAt);
        Assert.Equal(85m, dto.Marks);
        Assert.Equal(SubmissionStatus.Graded, dto.Status);
    }

    [Fact]
    public async Task Grade_AboveAssignmentMaxMarks_ThrowsDomainException()
    {
        var assignment = SeedAssignment(maxMarks: 50);
        var submission = SeedSubmission(assignment);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.GradeAsync(submission.Id, new GradeSubmissionRequest(51, null)));
        Assert.Equal(SubmissionStatus.Submitted, submission.Status);
    }

    [Fact]
    public async Task Grade_AnotherTeachersSubmission_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());
        var submission = SeedSubmission(assignment);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.GradeAsync(submission.Id, new GradeSubmissionRequest(10, null)));
    }

    [Fact]
    public async Task Grade_UnknownSubmission_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GradeAsync(Guid.NewGuid(), new GradeSubmissionRequest(10, null)));
    }

    [Fact]
    public async Task Return_ClearsMarksAndFeedbackAndSetsReturned()
    {
        var assignment = SeedAssignment();
        var submission = SeedSubmission(assignment);
        submission.Grade(85, "Well argued.", _teacherId);

        var dto = await _sut.ReturnAsync(submission.Id);

        Assert.Equal(SubmissionStatus.Returned, submission.Status);
        Assert.Null(submission.Marks);
        Assert.Null(submission.Feedback);
        Assert.Null(submission.GradedAt);
        Assert.Null(submission.GradedByTeacherId);
        Assert.Null(dto.Marks);
        Assert.Equal(SubmissionStatus.Returned, dto.Status);
    }

    [Fact]
    public async Task Return_NotYetGraded_ThrowsDomainException()
    {
        var assignment = SeedAssignment();
        var submission = SeedSubmission(assignment);

        await Assert.ThrowsAsync<DomainException>(() => _sut.ReturnAsync(submission.Id));
        Assert.Equal(SubmissionStatus.Submitted, submission.Status);
    }

    [Fact]
    public async Task GetForAssignment_AnotherTeachersAssignment_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());
        SeedSubmission(assignment);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetForAssignmentAsync(assignment.Id, new PageRequest(null), null));
    }

    [Fact]
    public async Task GetForAssignment_MarksSubmissionsAfterTheDeadlineAsLate()
    {
        var assignment = SeedAssignment(deadline: DateTimeOffset.UtcNow.AddDays(-1));
        SeedSubmission(assignment);

        var dto = Assert.Single((await _sut.GetForAssignmentAsync(assignment.Id, new PageRequest(null), null)).Items);

        Assert.True(dto.IsLate);
        Assert.Equal(_studentId, dto.StudentId);
    }

    [Fact]
    public async Task GetForAssignment_OnTimeSubmission_IsNotLate()
    {
        var assignment = SeedAssignment();
        SeedSubmission(assignment);

        var dto = Assert.Single((await _sut.GetForAssignmentAsync(assignment.Id, new PageRequest(null), null)).Items);

        Assert.False(dto.IsLate);
    }

    [Fact]
    public async Task GetForAssignment_PagesByStudentNameAndWalksTheCursor()
    {
        var assignment = SeedAssignment();
        var alice = SeedSubmissionForStudent(assignment, "Alice");
        var bob = SeedSubmissionForStudent(assignment, "Bob");
        var carol = SeedSubmissionForStudent(assignment, "Carol");

        var firstPage = await _sut.GetForAssignmentAsync(assignment.Id, new PageRequest(2), null);

        Assert.Equal([alice.Id, bob.Id], firstPage.Items.Select(s => s.Id).ToArray());
        Assert.True(firstPage.HasMore);
        Assert.NotNull(firstPage.NextCursor);

        var secondPage = await _sut.GetForAssignmentAsync(assignment.Id, new PageRequest(2), firstPage.NextCursor);

        Assert.Equal([carol.Id], secondPage.Items.Select(s => s.Id).ToArray());
        Assert.False(secondPage.HasMore);
        Assert.Null(secondPage.NextCursor);
    }

    private Submission SeedSubmissionForStudent(Assignment assignment, string fullName)
    {
        var submission = Submission.Create(assignment.Id, Guid.NewGuid(), "My answer");
        var student = AuthUser.CreatePending(fullName, $"{fullName}@example.com", "hash", UserRole.Student);
        typeof(Submission)
            .GetProperty(nameof(Submission.Student))!
            .SetValue(submission, student);
        _submissions.Items.Add(submission);
        return submission;
    }

    private sealed class FakeAssignmentRepository : IAssignmentRepository
    {

        public Task<AssignmentCounts> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(CountAssignments(null));

        public Task<AssignmentCounts> GetCountsByTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(CountAssignments(teacherId));

        public Task<List<Assignment>> GetRecentByTeacherAsync(Guid teacherId, int limit, CancellationToken ct = default)
            => Task.FromResult(Items
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList());

        private AssignmentCounts CountAssignments(Guid? teacherId)
        {
            var source = teacherId is null ? Items : Items.Where(a => a.TeacherId == teacherId);
            var drafts = source.Count(a => a.Status == AssignmentStatus.Draft);
            var published = source.Count(a => a.Status == AssignmentStatus.Published);
            return new AssignmentCounts(drafts + published, drafts, published);
        }
        public List<Assignment> Items { get; } = new();

        public Task<PagedResult<Assignment>> GetPageAsync(
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(Items));

        public Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(a => a.Id == id));

        public Task<PagedResult<Assignment>> GetPageByTeacherAsync(
            Guid teacherId,
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(
                Items.Where(a => a.TeacherId == teacherId).ToList()));

        public Task<PagedResult<Assignment>> GetPublishedPageForSectionAsync(
            Guid sectionId,
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(
                Items.Where(a => a.SectionId == sectionId && a.Status == AssignmentStatus.Published).ToList()));

        public Task<bool> HasSubmissionsAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Assignment assignment, CancellationToken ct = default)
        {
            Items.Add(assignment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Assignment assignment, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Assignment assignment, CancellationToken ct = default)
        {
            assignment.Delete();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSubmissionRepository : ISubmissionRepository
    {

        public Task<SubmissionCounts> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(new SubmissionCounts(
                Items.Count,
                Items.Count(s => s.Status == SubmissionStatus.Graded)));

        public Task<int> CountUngradedForTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(Items.Count(s => s.Status != SubmissionStatus.Graded));
        public List<Submission> Items { get; } = new();

        public Task<PagedResult<Submission>> GetPageAsync(
            int limit,
            DateTimeOffset? afterSubmittedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Submission>.FromAll(Items));

        public Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

        public Task<PagedResult<Submission>> GetPageByAssignmentAsync(
            Guid assignmentId,
            int limit,
            string? afterFullName,
            Guid? afterId,
            CancellationToken ct = default)
        {
            var ordered = Items
                .Where(s => s.AssignmentId == assignmentId)
                .OrderBy(s => s.Student?.FullName, StringComparer.Ordinal)
                .ThenBy(s => s.Id)
                .ToList();

            var rows = ordered
                .Where(s => afterFullName == null
                    || string.Compare(s.Student?.FullName, afterFullName, StringComparison.Ordinal) > 0
                    || (s.Student?.FullName == afterFullName && s.Id > afterId))
                .Take(limit + 1)
                .ToList();

            return Task.FromResult(PagedResult<Submission>.FromRows(
                rows, limit, last => CursorCodec.Encode(last.Student?.FullName ?? string.Empty, last.Id)));
        }

        public Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(s => s.AssignmentId == assignmentId && s.StudentId == studentId));

        public Task<List<Submission>> GetByStudentAndAssignmentIdsAsync(
            Guid studentId,
            IEnumerable<Guid> assignmentIds,
            CancellationToken ct = default)
        {
            var ids = assignmentIds.ToHashSet();
            return Task.FromResult(Items
                .Where(s => s.StudentId == studentId && ids.Contains(s.AssignmentId))
                .ToList());
        }

        public Task<Dictionary<Guid, SubmissionCounts>> GetCountsByAssignmentIdsAsync(
            IEnumerable<Guid> assignmentIds,
            CancellationToken ct = default)
        {
            var ids = assignmentIds.ToHashSet();
            return Task.FromResult(Items
                .Where(s => ids.Contains(s.AssignmentId))
                .GroupBy(s => s.AssignmentId)
                .ToDictionary(
                    g => g.Key,
                    g => new SubmissionCounts(g.Count(), g.Count(s => s.Status == SubmissionStatus.Graded))));
        }

        public Task<decimal?> GetMaxAwardedMarksAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(Items
                .Where(s => s.AssignmentId == assignmentId && s.Marks != null)
                .Max(s => s.Marks));

        public Task AddAsync(Submission submission, CancellationToken ct = default)
        {
            Items.Add(submission);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Submission submission, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {

        public Task<int> CountStudentsBySectionIdsAsync(IEnumerable<Guid> sectionIds, CancellationToken ct = default)
        {
            var ids = sectionIds.ToHashSet();
            return Task.FromResult(Students.Count(p => ids.Contains(p.SectionId)));
        }
        public List<StudentProfile> Students { get; } = new();

        public Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(Students.FirstOrDefault(p => p.AuthUserId == authUserId));

        public Task<List<StudentProfile>> GetStudentsByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
        {
            var ids = authUserIds.ToHashSet();
            return Task.FromResult(Students.Where(p => ids.Contains(p.AuthUserId)).ToList());
        }

        public Task<PagedResult<StudentProfile>> GetStudentsPageBySectionIdsAsync(
            IEnumerable<Guid> sectionIds,
            int limit,
            string? afterSectionName,
            string? afterFullName,
            Guid? afterId,
            CancellationToken ct = default)
        {
            return Task.FromResult(PagedResult<StudentProfile>.FromAll(
                Students.Where(p => sectionIds.Contains(p.SectionId)).ToList()));
        }

        public Task<TeacherProfile?> GetTeacherByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult<TeacherProfile?>(null);

        public Task<List<TeacherProfile>> GetTeachersByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
            => Task.FromResult(new List<TeacherProfile>());

        public Task<AdminProfile?> GetAdminByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult<AdminProfile?>(null);

        public Task AddAsync(TeacherProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task AddAsync(StudentProfile profile, CancellationToken ct = default)
        {
            Students.Add(profile);
            return Task.CompletedTask;
        }

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

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public string? UserId { get; set; }
    }
}