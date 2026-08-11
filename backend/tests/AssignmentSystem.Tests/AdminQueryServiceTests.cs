using AssignmentSystem.Application.Common.Interfaces;
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
        Assert.Equal(assignment.Status, dto.Status);
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

        public Task AddAsync(Assignment assignment, CancellationToken ct = default)
        {
            assignments.Add(assignment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Assignment assignment, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSubmissionRepository(List<Submission> submissions) : ISubmissionRepository
    {
        public Task<List<Submission>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(submissions);
    }
}
