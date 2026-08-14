using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class AdminStatsServiceTests
{
    [Fact]
    public async Task GetOverview_ReturnsRoleStatusAndActivityCounts()
    {
        var student = AuthUser.CreatePending("Student One", "student@example.com", "hash", UserRole.Student);
        var teacher = AuthUser.CreatePending("Teacher One", "teacher@example.com", "hash", UserRole.Teacher);
        var admin = AuthUser.CreateApprovedAdmin("Admin One", "admin@example.com", "hash");
        ((ICreatable)student).CreatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        ((ICreatable)teacher).CreatedAt = DateTimeOffset.UtcNow.AddDays(-2);

        var grade = Grade.Create("Grade 1", "2026");
        var section = Section.Create("Section A", grade.Id);
        var subject = Subject.Create("Mathematics", null, grade.Id);

        var draft = Assignment.Create("Draft", section.Id, subject.Id, teacher.Id,
            DateTimeOffset.UtcNow.AddDays(7), 100);
        var published = Assignment.Create("Published", section.Id, subject.Id, teacher.Id,
            DateTimeOffset.UtcNow.AddDays(7), 100);
        published.Publish();

        var gradedSubmission = Submission.Create(published.Id, student.Id, content: "Answer");
        gradedSubmission.Grade(17, "Good", teacher.Id);

        var sut = new AdminStatsService(
            new FakeUserRepository([student, teacher, admin]),
            new FakeGradeRepository([grade]),
            new FakeSectionRepository([section]),
            new FakeSubjectRepository([subject]),
            new FakeAssignmentRepository([draft, published]),
            new FakeSubmissionRepository([gradedSubmission, Submission.Create(published.Id, student.Id, content: "Ungraded")]));

        var overview = await sut.GetOverviewAsync();

        Assert.Equal(1, overview.Students);
        Assert.Equal(1, overview.Teachers);
        Assert.Equal(1, overview.Admins);
        Assert.Equal(2, overview.Pending);
        Assert.Equal(1, overview.Grades);
        Assert.Equal(1, overview.Sections);
        Assert.Equal(1, overview.Subjects);
        Assert.Equal(2, overview.Assignments);
        Assert.Equal(1, overview.Drafts);
        Assert.Equal(1, overview.Published);
        Assert.Equal(2, overview.Submissions);
        Assert.Equal(1, overview.Graded);
        Assert.Equal(1, overview.Ungraded);
        Assert.Equal(2, overview.RecentPending.Count);
        Assert.Equal(student.Id, overview.RecentPending[0].Id);
    }

    private sealed class FakeUserRepository(List<AuthUser> users) : IUserRepository
    {
        public Task<UserCounts> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(new UserCounts(
                users.Count(u => u.Role == UserRole.Student),
                users.Count(u => u.Role == UserRole.Teacher),
                users.Count(u => u.Role == UserRole.Admin),
                users.Count(u => u.Status == AccountStatus.Pending)));

        public Task<List<AuthUser>> GetRecentPendingAsync(int limit, CancellationToken ct = default)
            => Task.FromResult(users
                .Where(u => u.Status == AccountStatus.Pending)
                .OrderByDescending(u => u.CreatedAt)
                .Take(limit)
                .ToList());

        public Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(users.FirstOrDefault(u => u.Id == id));

        public Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(users.FirstOrDefault(u => u.Email == email));

        public Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
            => Task.FromResult(users.FirstOrDefault(u =>
                u.RefreshToken == refreshToken || u.PreviousRefreshToken == refreshToken));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(users.Any(u => u.Email == email));

        public Task<PagedResult<AuthUser>> GetPageAsync(
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            AccountStatus? status,
            UserRole? role,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<AuthUser>.FromAll(users));

        public Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<int> CountUsableAdminsAsync(CancellationToken ct = default)
            => Task.FromResult(users.Count(u => u.Role == UserRole.Admin && u.Status == AccountStatus.Approved && u.IsActive));

        public Task AddAsync(AuthUser user, CancellationToken ct = default)
        {
            users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AuthUser user, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeGradeRepository(List<Grade> grades) : IGradeRepository
    {
        public Task<List<Grade>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(grades);

        public Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(grades.FirstOrDefault(g => g.Id == id));

        public Task<Dictionary<Guid, GradeCounts>> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(new Dictionary<Guid, GradeCounts>());

        public Task<List<Grade>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => Task.FromResult(grades.Where(g => ids.Contains(g.Id)).ToList());

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(grades.Any(g => g.Id == id));

        public Task<bool> ExistsAsync(string name, string academicYear, CancellationToken ct = default)
            => Task.FromResult(grades.Any(g => g.Name == name && g.AcademicYear == academicYear));

        public Task<bool> HasSubjectsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasSectionsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Grade grade, CancellationToken ct = default)
        {
            grades.Add(grade);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Grade grade, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSectionRepository(List<Section> sections) : ISectionRepository
    {
        public Task<List<Section>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(sections);

        public Task<Section?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(sections.FirstOrDefault(s => s.Id == id));

        public Task<Dictionary<Guid, SectionCounts>> GetCountsAsync(CancellationToken ct = default)
            => Task.FromResult(new Dictionary<Guid, SectionCounts>());

        public Task<List<Section>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => Task.FromResult(sections.Where(s => ids.Contains(s.Id)).ToList());

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(sections.Any(s => s.Id == id));

        public Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
            => Task.FromResult(sections.Any(s => s.Name == name && s.GradeId == gradeId));

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Section section, CancellationToken ct = default)
        {
            sections.Add(section);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Section section, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSubjectRepository(List<Subject> subjects) : ISubjectRepository
    {
        public Task<List<Subject>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(subjects);

        public Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(subjects.FirstOrDefault(s => s.Id == id));

        public Task<Dictionary<Guid, int>> GetTeacherCountsAsync(CancellationToken ct = default)
            => Task.FromResult(new Dictionary<Guid, int>());

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(subjects.Any(s => s.Id == id));

        public Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
            => Task.FromResult(subjects.Any(s => s.Name == name && s.GradeId == gradeId));

        public Task<bool> HasAssignmentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Subject subject, CancellationToken ct = default)
        {
            subjects.Add(subject);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Subject subject, CancellationToken ct = default)
            => Task.CompletedTask;
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
}
