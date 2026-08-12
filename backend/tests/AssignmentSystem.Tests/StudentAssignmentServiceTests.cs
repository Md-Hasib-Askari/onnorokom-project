using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Student;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class StudentAssignmentServiceTests
{
    private readonly FakeAssignmentRepository _assignments = new();
    private readonly FakeSubmissionRepository _submissions = new();
    private readonly FakeProfileRepository _profiles = new();
    private readonly FakeCurrentUser _currentUser = new();
    private readonly StudentAssignmentService _sut;

    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _sectionId = Guid.NewGuid();

    public StudentAssignmentServiceTests()
    {
        _currentUser.UserId = _studentId.ToString();
        _profiles.Students.Add(StudentProfile.Create(_studentId, _sectionId));
        _sut = new StudentAssignmentService(_assignments, _submissions, _profiles, _currentUser);
    }

    private Assignment SeedAssignment(
        bool published = true,
        Guid? sectionId = null,
        DateTimeOffset? deadline = null,
        bool allowLateSubmission = false)
    {
        var assignment = Assignment.Create("Essay", sectionId ?? _sectionId, Guid.NewGuid(), Guid.NewGuid(),
            deadline ?? DateTimeOffset.UtcNow.AddDays(7), 100, allowLateSubmission: allowLateSubmission);

        if (published)
        {
            assignment.Publish();
        }

        _assignments.Items.Add(assignment);
        return assignment;
    }

    private static SubmissionCreateRequest Answer(string content = "My answer") => new(content, null);

    [Fact]
    public async Task GetMyAssignments_ExcludesDrafts()
    {
        SeedAssignment(published: false);
        var published = SeedAssignment();

        var list = await _sut.GetMyAssignmentsAsync();

        Assert.Equal(published.Id, Assert.Single(list).Id);
    }

    [Fact]
    public async Task GetById_DraftAssignment_ThrowsEntityNotFoundException()
    {
        var draft = SeedAssignment(published: false);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(draft.Id));
    }

    [Fact]
    public async Task GetMyAssignments_ExcludesOtherSections()
    {
        SeedAssignment(sectionId: Guid.NewGuid());

        Assert.Empty(await _sut.GetMyAssignmentsAsync());
    }

    [Fact]
    public async Task GetById_AnotherSectionsAssignment_ThrowsEntityNotFoundException()
    {
        var other = SeedAssignment(sectionId: Guid.NewGuid());

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.GetByIdAsync(other.Id));
    }

    [Fact]
    public async Task GetMyAssignments_CarriesTheStudentsOwnSubmissionState()
    {
        var assignment = SeedAssignment();
        var submission = Submission.Create(assignment.Id, _studentId, "My answer");
        submission.Grade(90, "Good work.", Guid.NewGuid());
        _submissions.Items.Add(submission);

        var item = Assert.Single(await _sut.GetMyAssignmentsAsync());

        Assert.Equal(SubmissionStatus.Graded, item.SubmissionStatus);
        Assert.Equal(90m, item.Marks);
    }

    [Fact]
    public async Task GetMyAssignments_NoSubmission_LeavesStatusNull()
    {
        SeedAssignment();

        var item = Assert.Single(await _sut.GetMyAssignmentsAsync());

        Assert.Null(item.SubmissionStatus);
        Assert.False(item.IsLate);
    }

    [Fact]
    public async Task GetMyAssignments_WithoutAStudentProfile_ThrowsForbiddenException()
    {
        _profiles.Students.Clear();

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetMyAssignmentsAsync());
    }

    [Fact]
    public async Task Submit_BeforeTheDeadline_CreatesASubmittedRow()
    {
        var assignment = SeedAssignment();

        var dto = await _sut.SubmitAsync(assignment.Id, Answer());

        var stored = Assert.Single(_submissions.Items);
        Assert.Equal(SubmissionStatus.Submitted, stored.Status);
        Assert.Equal("My answer", stored.Content);
        Assert.Equal(SubmissionStatus.Submitted, dto.SubmissionStatus);
        Assert.False(dto.IsLate);
        Assert.False(dto.CanSubmit);
        Assert.True(dto.CanEdit);
    }

    [Fact]
    public async Task Submit_PastTheDeadlineWithLateSubmissionsOff_ThrowsDomainException()
    {
        var assignment = SeedAssignment(deadline: DateTimeOffset.UtcNow.AddDays(-1));

        await Assert.ThrowsAsync<DomainException>(() => _sut.SubmitAsync(assignment.Id, Answer()));
        Assert.Empty(_submissions.Items);
    }

    [Fact]
    public async Task Submit_PastTheDeadlineWithLateSubmissionsOn_IsAcceptedAndFlaggedLate()
    {
        var assignment = SeedAssignment(deadline: DateTimeOffset.UtcNow.AddDays(-1), allowLateSubmission: true);

        var dto = await _sut.SubmitAsync(assignment.Id, Answer());

        Assert.Single(_submissions.Items);
        Assert.True(dto.IsLate);
        Assert.True(dto.IsPastDeadline);
        Assert.Equal(SubmissionStatus.Submitted, dto.SubmissionStatus);
    }

    [Fact]
    public async Task Submit_Twice_RevisesTheSameRowRatherThanInsertingAnother()
    {
        var assignment = SeedAssignment();
        await _sut.SubmitAsync(assignment.Id, Answer());

        var dto = await _sut.SubmitAsync(assignment.Id, Answer("A better answer"));

        var stored = Assert.Single(_submissions.Items);
        Assert.Equal(SubmissionStatus.Resubmitted, stored.Status);
        Assert.Equal("A better answer", stored.Content);
        Assert.Equal(SubmissionStatus.Resubmitted, dto.SubmissionStatus);
    }

    [Fact]
    public async Task Submit_ADraftAssignment_ThrowsEntityNotFoundException()
    {
        var draft = SeedAssignment(published: false);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.SubmitAsync(draft.Id, Answer()));
        Assert.Empty(_submissions.Items);
    }

    [Fact]
    public async Task UpdateSubmission_AfterGrading_ThrowsDomainException()
    {
        var assignment = SeedAssignment();
        await _sut.SubmitAsync(assignment.Id, Answer());
        _submissions.Items[0].Grade(80, "Solid.", Guid.NewGuid());

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateSubmissionAsync(assignment.Id, new SubmissionUpdateRequest("Sneaky edit", null)));
        Assert.Equal("My answer", _submissions.Items[0].Content);
    }

    [Fact]
    public async Task UpdateSubmission_AfterReturnForRevision_IsAllowed()
    {
        var assignment = SeedAssignment();
        await _sut.SubmitAsync(assignment.Id, Answer());
        _submissions.Items[0].Grade(80, "Needs more depth.", Guid.NewGuid());
        _submissions.Items[0].ReturnForRevision();

        var dto = await _sut.UpdateSubmissionAsync(assignment.Id, new SubmissionUpdateRequest("Reworked answer", null));

        Assert.Equal(SubmissionStatus.Resubmitted, _submissions.Items[0].Status);
        Assert.Equal("Reworked answer", _submissions.Items[0].Content);
        Assert.Equal(SubmissionStatus.Resubmitted, dto.SubmissionStatus);
    }

    [Fact]
    public async Task UpdateSubmission_WithNothingSubmittedYet_ThrowsEntityNotFoundException()
    {
        var assignment = SeedAssignment();

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateSubmissionAsync(assignment.Id, new SubmissionUpdateRequest("Edit", null)));
        Assert.Empty(_submissions.Items);
    }

    [Fact]
    public async Task GetById_GradedSubmission_ReportsNeitherSubmitNorEdit()
    {
        var assignment = SeedAssignment();
        await _sut.SubmitAsync(assignment.Id, Answer());
        _submissions.Items[0].Grade(75, "Well done.", Guid.NewGuid());

        var dto = await _sut.GetByIdAsync(assignment.Id);

        Assert.False(dto.CanSubmit);
        Assert.False(dto.CanEdit);
        Assert.Equal(75m, dto.Marks);
        Assert.Equal("Well done.", dto.Feedback);
    }

    [Fact]
    public async Task GetById_PastTheDeadlineWithNothingSubmitted_ReportsNeitherSubmitNorEdit()
    {
        var assignment = SeedAssignment(deadline: DateTimeOffset.UtcNow.AddDays(-1));

        var dto = await _sut.GetByIdAsync(assignment.Id);

        Assert.False(dto.CanSubmit);
        Assert.False(dto.CanEdit);
        Assert.True(dto.IsPastDeadline);
    }

    [Fact]
    public async Task GetById_OpenAssignmentWithNothingSubmitted_ReportsCanSubmit()
    {
        var assignment = SeedAssignment();

        var dto = await _sut.GetByIdAsync(assignment.Id);

        Assert.True(dto.CanSubmit);
        Assert.False(dto.CanEdit);
    }

    private sealed class FakeAssignmentRepository : IAssignmentRepository
    {
        public List<Assignment> Items { get; } = [];

        public Task<List<Assignment>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Items.ToList());

        public Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(a => a.Id == id));

        public Task<List<Assignment>> GetByTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(a => a.TeacherId == teacherId).ToList());

        public Task<List<Assignment>> GetPublishedForSectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.FromResult(Items
                .Where(a => a.SectionId == sectionId && a.Status == AssignmentStatus.Published)
                .ToList());

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
        public List<Submission> Items { get; } = [];

        public Task<List<Submission>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Items.ToList());

        public Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

        public Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(Items.Where(s => s.AssignmentId == assignmentId).ToList());

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
        public List<StudentProfile> Students { get; } = [];

        public Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(Students.FirstOrDefault(p => p.AuthUserId == authUserId));

        public Task<List<StudentProfile>> GetStudentsByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
        {
            var ids = authUserIds.ToHashSet();
            return Task.FromResult(Students.Where(p => ids.Contains(p.AuthUserId)).ToList());
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