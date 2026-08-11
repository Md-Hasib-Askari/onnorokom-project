using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.Services;
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
        var dto = (await sut.GetAllAssignmentsAsync()).Single();

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

        var dtos = await sut.GetAllAssignmentsAsync();

        Assert.Equal(2, dtos.Single(d => d.Id == counted.Id).SubmissionCount);
        Assert.Equal(0, dtos.Single(d => d.Id == untouched.Id).SubmissionCount);
    }

    [Fact]
    public async Task GetAllSubmissions_MapsSubmissionFields()
    {
        var submission = Submission.Create(Guid.NewGuid(), Guid.NewGuid(), content: "My answer");
        var repo = new FakeSubmissionRepository([submission]);

        var sut = new AdminQueryService(new FakeAssignmentRepository([]), repo, TestMappers.CreateMapper());
        var dto = (await sut.GetAllSubmissionsAsync()).Single();

        Assert.Equal(submission.Id, dto.Id);
        Assert.Equal(submission.AssignmentId, dto.AssignmentId);
        Assert.Equal(submission.StudentId, dto.StudentId);
        Assert.Equal(submission.Content, dto.Content);
        Assert.Equal(submission.Status, dto.Status);
        Assert.Equal(submission.SubmittedAt, dto.SubmittedAt);
    }

    private sealed class FakeAssignmentRepository(List<Assignment> assignments) : IAssignmentRepository
    {
        public Task<List<Assignment>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(assignments);

        public Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(assignments.FirstOrDefault(a => a.Id == id));

        public Task<List<Assignment>> GetByTeacherAsync(Guid teacherId, CancellationToken ct = default)
            => Task.FromResult(assignments.Where(a => a.TeacherId == teacherId).ToList());

        public Task<List<Assignment>> GetPublishedForSectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.FromResult(assignments
                .Where(a => a.SectionId == sectionId && a.Status == AssignmentStatus.Published)
                .ToList());

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
        public Task<List<Submission>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(submissions);

        public Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(submissions.FirstOrDefault(s => s.Id == id));

        public Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
            => Task.FromResult(submissions.Where(s => s.AssignmentId == assignmentId).ToList());

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
