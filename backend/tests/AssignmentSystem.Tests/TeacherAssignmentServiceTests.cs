using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class TeacherAssignmentServiceTests
{
    private readonly FakeSubmissionRepository _submissions = new();
    private readonly FakeAssignmentRepository _assignments;
    private readonly FakeSectionSubjectRepository _sectionSubjects = new();
    private readonly FakeProfileRepository _profiles = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly TeacherAssignmentService _sut;

    private readonly Guid _teacherId = Guid.NewGuid();
    private readonly Guid _sectionId = Guid.NewGuid();
    private readonly Guid _subjectId = Guid.NewGuid();

    public TeacherAssignmentServiceTests()
    {
        _currentUser.UserId = _teacherId.ToString();
        _assignments = new FakeAssignmentRepository(_submissions);
        _sut = new TeacherAssignmentService(_assignments, _submissions, _sectionSubjects, _profiles, _currentUser);
    }

    private void GiveTeacherTheSectionSubject(Guid? teacherId = null)
        => _sectionSubjects.Rows.Add(SectionSubject.Create(_sectionId, _subjectId, teacherId ?? _teacherId));

    private Assignment SeedAssignment(Guid? teacherId = null, decimal maxMarks = 100)
    {
        var assignment = Assignment.Create("Essay", _sectionId, _subjectId, teacherId ?? _teacherId,
            DateTimeOffset.UtcNow.AddDays(7), maxMarks);
        _assignments.Items.Add(assignment);
        return assignment;
    }

    private static AssignmentUpdateRequest UpdateRequest(decimal maxMarks = 100)
        => new("Essay v2", "Revised brief", DateTimeOffset.UtcNow.AddDays(14), maxMarks, false);

    [Fact]
    public async Task Create_TeacherDoesNotHoldSectionSubject_ThrowsForbiddenException()
    {
        GiveTeacherTheSectionSubject(teacherId: Guid.NewGuid());
        var request = new AssignmentCreateRequest("Essay", null, _sectionId, _subjectId,
            DateTimeOffset.UtcNow.AddDays(7), 100, false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.CreateAsync(request));
        Assert.Empty(_assignments.Items);
    }

    [Fact]
    public async Task Create_TeacherHoldsSectionSubject_SavesDraftOwnedByCaller()
    {
        GiveTeacherTheSectionSubject();
        var request = new AssignmentCreateRequest("Essay", null, _sectionId, _subjectId,
            DateTimeOffset.UtcNow.AddDays(7), 100, false);

        var dto = await _sut.CreateAsync(request);

        var saved = _assignments.Items.Single();
        Assert.Equal(_teacherId, saved.TeacherId);
        Assert.Equal(_sectionId, saved.SectionId);
        Assert.Equal(AssignmentStatus.Draft, dto.Status);
    }

    [Fact]
    public async Task Update_AnotherTeachersAssignment_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.UpdateAsync(assignment.Id, UpdateRequest()));
    }

    [Fact]
    public async Task Update_MaxMarksBelowAnAwardedMark_ThrowsDomainException()
    {
        var assignment = SeedAssignment();
        var submission = Submission.Create(assignment.Id, Guid.NewGuid());
        submission.Grade(80, null, _teacherId);
        _submissions.Items.Add(submission);

        await Assert.ThrowsAsync<DomainException>(() => _sut.UpdateAsync(assignment.Id, UpdateRequest(maxMarks: 50)));
        Assert.Equal(100m, assignment.MaxMarks);
    }

    [Fact]
    public async Task Update_MaxMarksAtOrAboveTheHighestAwardedMark_Succeeds()
    {
        var assignment = SeedAssignment();
        var submission = Submission.Create(assignment.Id, Guid.NewGuid());
        submission.Grade(80, null, _teacherId);
        _submissions.Items.Add(submission);

        var dto = await _sut.UpdateAsync(assignment.Id, UpdateRequest(maxMarks: 80));

        Assert.Equal(80m, dto.MaxMarks);
        Assert.Equal("Essay v2", dto.Title);
    }

    [Fact]
    public async Task Publish_Twice_ThrowsDomainException()
    {
        var assignment = SeedAssignment();

        var dto = await _sut.PublishAsync(assignment.Id);
        Assert.Equal(AssignmentStatus.Published, dto.Status);

        await Assert.ThrowsAsync<DomainException>(() => _sut.PublishAsync(assignment.Id));
    }

    [Fact]
    public async Task Unpublish_WhenDraft_ThrowsDomainException()
    {
        var assignment = SeedAssignment();

        await Assert.ThrowsAsync<DomainException>(() => _sut.UnpublishAsync(assignment.Id));
    }

    [Fact]
    public async Task Unpublish_WhenPublished_SetsStatusToDraft()
    {
        var assignment = SeedAssignment();
        await _sut.PublishAsync(assignment.Id);

        var dto = await _sut.UnpublishAsync(assignment.Id);

        Assert.Equal(AssignmentStatus.Draft, dto.Status);
    }

    [Fact]
    public async Task Unpublish_AnotherTeachersAssignment_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.UnpublishAsync(assignment.Id));
    }

    [Fact]
    public async Task CloseSubmissions_WhenOpen_SetsSubmissionsOpenToFalse()
    {
        var assignment = SeedAssignment();
        await _sut.PublishAsync(assignment.Id);

        var dto = await _sut.CloseSubmissionsAsync(assignment.Id);

        Assert.False(dto.SubmissionsOpen);
    }

    [Fact]
    public async Task CloseSubmissions_WhenAlreadyClosed_ThrowsDomainException()
    {
        var assignment = SeedAssignment();
        await _sut.PublishAsync(assignment.Id);
        await _sut.CloseSubmissionsAsync(assignment.Id);

        await Assert.ThrowsAsync<DomainException>(() => _sut.CloseSubmissionsAsync(assignment.Id));
    }

    [Fact]
    public async Task CloseSubmissions_AnotherTeachersAssignment_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.CloseSubmissionsAsync(assignment.Id));
    }

    [Fact]
    public async Task ReopenSubmissions_WhenClosed_SetsSubmissionsOpenToTrue()
    {
        var assignment = SeedAssignment();
        await _sut.PublishAsync(assignment.Id);
        await _sut.CloseSubmissionsAsync(assignment.Id);

        var dto = await _sut.ReopenSubmissionsAsync(assignment.Id);

        Assert.True(dto.SubmissionsOpen);
    }

    [Fact]
    public async Task ReopenSubmissions_WhenAlreadyOpen_ThrowsDomainException()
    {
        var assignment = SeedAssignment();

        await Assert.ThrowsAsync<DomainException>(() => _sut.ReopenSubmissionsAsync(assignment.Id));
    }

    [Fact]
    public async Task ReopenSubmissions_AnotherTeachersAssignment_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.ReopenSubmissionsAsync(assignment.Id));
    }

    [Fact]
    public async Task Delete_AnotherTeachersAssignment_ThrowsForbiddenException()
    {
        var assignment = SeedAssignment(teacherId: Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(assignment.Id));
        Assert.Single(_assignments.Items);
    }

    [Fact]
    public async Task Delete_WithSubmissions_ThrowsEntityInUseException()
    {
        var assignment = SeedAssignment();
        _submissions.Items.Add(Submission.Create(assignment.Id, Guid.NewGuid()));

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(assignment.Id));
        Assert.Single(_assignments.Items);
    }

    [Fact]
    public async Task Delete_WithoutSubmissions_SoftDeletes()
    {
        var assignment = SeedAssignment();

        await _sut.DeleteAsync(assignment.Id);

        Assert.True(assignment.IsDeleted);
    }

    [Fact]
    public async Task GetById_UnknownAssignment_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetMyAssignments_ReturnsOnlyOwnAssignmentsWithSubmissionCounts()
    {
        var mine = SeedAssignment();
        SeedAssignment(teacherId: Guid.NewGuid());
        _submissions.Items.Add(Submission.Create(mine.Id, Guid.NewGuid()));
        var graded = Submission.Create(mine.Id, Guid.NewGuid());
        graded.Grade(70, null, _teacherId);
        _submissions.Items.Add(graded);

        var page = await _sut.GetMyAssignmentsAsync(new PageRequest(null), null);

        var dto = Assert.Single(page.Items);
        Assert.Equal(mine.Id, dto.Id);
        Assert.Equal(2, dto.SubmissionCount);
        Assert.Equal(1, dto.GradedCount);
    }

    [Fact]
    public async Task GetMySectionSubjects_ReturnsOnlyLinksOwnedByCaller()
    {
        GiveTeacherTheSectionSubject();
        _sectionSubjects.Rows.Add(SectionSubject.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        var page = await _sut.GetMySectionSubjectsAsync();

        Assert.False(page.HasMore);
        Assert.Null(page.NextCursor);
        var link = Assert.Single(page.Items);
        Assert.Equal(_sectionId, link.SectionId);
        Assert.Equal(_subjectId, link.SubjectId);
    }

    [Fact]
    public async Task GetMyStudents_ReturnsOnlyStudentsInTaughtSections()
    {
        GiveTeacherTheSectionSubject();
        var otherSectionId = Guid.NewGuid();

        var mine = StudentProfile.Create(Guid.NewGuid(), _sectionId);
        mine.UpdateDetails("R-101", null, null, null, null, null, null);
        _profiles.Students.Add(mine);

        var other = StudentProfile.Create(Guid.NewGuid(), otherSectionId);
        _profiles.Students.Add(other);

        var page = await _sut.GetMyStudentsAsync(new PageRequest(null), null);

        var student = Assert.Single(page.Items);
        Assert.Equal(mine.AuthUserId, student.Id);
        Assert.Equal("R-101", student.RollNumber);
        Assert.Equal(_sectionId, student.SectionId);
    }

    [Fact]
    public async Task GetMyStudents_PagesBySectionThenNameAndWalksTheCursor()
    {
        GiveTeacherTheSectionSubject();

        var first = SeedTaughtStudent("Beta", "Alice");
        var second = SeedTaughtStudent("Alpha", "Zoe");
        var third = SeedTaughtStudent("Alpha", "Bob");

        var firstPage = await _sut.GetMyStudentsAsync(new PageRequest(2), null);

        // Ordered by (Section.Name, FullName, Id): Alpha/Bob, Alpha/Zoe, Beta/Alice.
        Assert.Equal([third.Id, second.Id], firstPage.Items.Select(s => s.Id).ToArray());
        Assert.True(firstPage.HasMore);
        Assert.NotNull(firstPage.NextCursor);

        var secondPage = await _sut.GetMyStudentsAsync(new PageRequest(2), firstPage.NextCursor);

        Assert.Equal([first.Id], secondPage.Items.Select(s => s.Id).ToArray());
        Assert.False(secondPage.HasMore);
        Assert.Null(secondPage.NextCursor);
    }

    private TeacherStudentDto SeedTaughtStudent(string sectionName, string fullName)
    {
        var student = StudentProfile.Create(Guid.NewGuid(), _sectionId);
        var section = Section.Create(sectionName, Guid.NewGuid());
        var user = AuthUser.CreatePending(fullName, $"{fullName}@example.com", "hash", UserRole.Student);
        typeof(StudentProfile).GetProperty(nameof(StudentProfile.Section))!.SetValue(student, section);
        typeof(StudentProfile).GetProperty(nameof(StudentProfile.AuthUser))!.SetValue(student, user);
        _profiles.Students.Add(student);
        return new TeacherStudentDto(
            student.AuthUserId,
            user.FullName,
            student.RollNumber,
            student.SectionId,
            section.Name,
            null);
    }

    private sealed class FakeAssignmentRepository(FakeSubmissionRepository submissions) : IAssignmentRepository
    {

        public Task<AssignmentCounts> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(CountAssignments(null));

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
        {
            var ordered = Items
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ToList();

            var rows = ordered
                .Where(a => afterCreatedAt == null
                    || a.CreatedAt < afterCreatedAt
                    || (a.CreatedAt == afterCreatedAt && a.Id < afterId))
                .Take(limit + 1)
                .ToList();

            return Task.FromResult(PagedResult<Assignment>.FromRows(
                rows, limit, last => CursorCodec.Encode(last.CreatedAt, last.Id)));
        }

        public Task<PagedResult<Assignment>> GetPublishedPageForSectionAsync(
            Guid sectionId,
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<Assignment>.FromAll(
                Items.Where(a => a.SectionId == sectionId && a.Status == AssignmentStatus.Published).ToList()));

        public Task<bool> HasSubmissionsAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(submissions.Items.Any(s => s.AssignmentId == assignmentId));

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
            => Task.FromResult(PagedResult<Submission>.FromAll(
                Items.Where(s => s.AssignmentId == assignmentId).ToList()));

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

    private sealed class FakeSectionSubjectRepository : ISectionSubjectRepository
    {
        public List<SectionSubject> Rows { get; } = new();

        public Task<List<SectionSubject>> GetBySectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.FromResult(Rows.Where(r => r.SectionId == sectionId).ToList());

        public Task<SectionSubject?> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default)
            => Task.FromResult(Rows.FirstOrDefault(r => r.SectionId == sectionId && r.SubjectId == subjectId));

        public Task<List<SectionSubject>> GetByTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(Rows.Where(r => r.TeacherId == teacherId).ToList());

        public Task<bool> ExistsForTeacherAsync(Guid sectionId, Guid subjectId, Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(Rows.Any(r => r.SectionId == sectionId && r.SubjectId == subjectId && r.TeacherId == teacherId));

        public Task AddAsync(SectionSubject sectionSubject, CancellationToken ct = default)
        {
            Rows.Add(sectionSubject);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SectionSubject sectionSubject, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SoftDeleteForSectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SoftDeleteForSubjectAsync(Guid subjectId, CancellationToken ct = default)
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
            var ids = sectionIds.ToHashSet();
            var ordered = Students
                .Where(p => ids.Contains(p.SectionId))
                .OrderBy(p => p.Section?.Name, StringComparer.Ordinal)
                .ThenBy(p => p.AuthUser?.FullName, StringComparer.Ordinal)
                .ThenBy(p => p.Id)
                .ToList();

            var rows = ordered
                .Where(p => afterSectionName == null
                    || string.Compare(p.Section?.Name, afterSectionName, StringComparison.Ordinal) > 0
                    || (p.Section?.Name == afterSectionName
                        && (string.Compare(p.AuthUser?.FullName, afterFullName, StringComparison.Ordinal) > 0
                            || (p.AuthUser?.FullName == afterFullName && p.Id > afterId))))
                .Take(limit + 1)
                .ToList();

            return Task.FromResult(PagedResult<StudentProfile>.FromRows(
                rows,
                limit,
                last => CursorCodec.Encode(
                    last.Section?.Name ?? string.Empty,
                    last.AuthUser?.FullName ?? string.Empty,
                    last.Id)));
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