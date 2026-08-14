using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class SectionSubjectServiceTests
{
    private readonly FakeSectionSubjectRepository _sectionSubjects = new();
    private readonly FakeSectionRepository _sections = new();
    private readonly FakeSubjectRepository _subjects = new();
    private readonly FakeUserRepository _users = new();
    private readonly SectionSubjectService _sut;

    public SectionSubjectServiceTests()
    {
        _sut = new SectionSubjectService(_sectionSubjects, _sections, _subjects, _users);
    }

    private (Grade Grade, Section Section, Subject Subject) SeedGradeSectionSubject()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var section = Section.Create("Section A", grade.Id);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _sections.Sections.Add(section);
        _subjects.Subjects.Add(subject);
        return (grade, section, subject);
    }

    private AuthUser AddApprovedTeacher()
    {
        var teacher = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        teacher.Approve();
        _users.Users.Add(teacher);
        return teacher;
    }

    [Fact]
    public async Task AssignTeacher_NoExistingRow_CreatesRowAndSetsTeacher()
    {
        var (_, section, subject) = SeedGradeSectionSubject();
        var teacher = AddApprovedTeacher();

        var dto = await _sut.AssignTeacherAsync(section.Id, subject.Id, teacher.Id);

        var row = _sectionSubjects.Rows.Single();
        Assert.Equal(teacher.Id, row.TeacherId);
        Assert.Equal(teacher.Id, dto.TeacherId);
        Assert.Equal(subject.Id, dto.SubjectId);
    }

    [Fact]
    public async Task AssignTeacher_ExistingRow_UpdatesTeacher()
    {
        var (_, section, subject) = SeedGradeSectionSubject();
        var firstTeacher = AddApprovedTeacher();
        _sectionSubjects.Rows.Add(SectionSubject.Create(section.Id, subject.Id, firstTeacher.Id));
        var newTeacher = AddApprovedTeacher();

        var dto = await _sut.AssignTeacherAsync(section.Id, subject.Id, newTeacher.Id);

        Assert.Single(_sectionSubjects.Rows);
        Assert.Equal(newTeacher.Id, dto.TeacherId);
    }

    [Fact]
    public async Task AssignTeacher_NonTeacher_ThrowsInvalidTeacherException()
    {
        var (_, section, subject) = SeedGradeSectionSubject();
        var student = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        student.Approve();
        _users.Users.Add(student);

        await Assert.ThrowsAsync<InvalidTeacherException>(
            () => _sut.AssignTeacherAsync(section.Id, subject.Id, student.Id));
    }

    [Fact]
    public async Task AssignTeacher_UnknownSection_ThrowsEntityNotFoundException()
    {
        var teacher = AddApprovedTeacher();

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.AssignTeacherAsync(Guid.NewGuid(), Guid.NewGuid(), teacher.Id));
    }

    [Fact]
    public async Task AssignTeacher_SubjectFromDifferentGrade_ThrowsDomainException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var otherGrade = Grade.Create("Grade 7", "2026");
        var section = Section.Create("Section A", grade.Id);
        var subject = Subject.Create("Mathematics", "MATH-7", otherGrade.Id);
        _sections.Sections.Add(section);
        _subjects.Subjects.Add(subject);
        var teacher = AddApprovedTeacher();

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.AssignTeacherAsync(section.Id, subject.Id, teacher.Id));
    }

    [Fact]
    public async Task UnassignTeacher_ClearsTeacher()
    {
        var (_, section, subject) = SeedGradeSectionSubject();
        var teacher = AddApprovedTeacher();
        _sectionSubjects.Rows.Add(SectionSubject.Create(section.Id, subject.Id, teacher.Id));

        var dto = await _sut.UnassignTeacherAsync(section.Id, subject.Id);

        Assert.Null(_sectionSubjects.Rows.Single().TeacherId);
        Assert.Null(dto.TeacherId);
    }

    [Fact]
    public async Task GetSectionSubjects_ReturnsOneRowPerGradeSubjectWithAssignmentsMerged()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var section = Section.Create("Section A", grade.Id);
        var assignedSubject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        var unassignedSubject = Subject.Create("Science", "SCI-6", grade.Id);
        _sections.Sections.Add(section);
        _subjects.Subjects.Add(assignedSubject);
        _subjects.Subjects.Add(unassignedSubject);
        var teacher = AddApprovedTeacher();
        _sectionSubjects.Rows.Add(SectionSubject.Create(section.Id, assignedSubject.Id, teacher.Id));

        var page = await _sut.GetSectionSubjectsAsync(section.Id);

        Assert.Equal(2, page.Items.Count);
        Assert.False(page.HasMore);
        Assert.Null(page.NextCursor);
        var assignedDto = page.Items.Single(r => r.SubjectId == assignedSubject.Id);
        Assert.Equal(teacher.Id, assignedDto.TeacherId);
        var unassignedDto = page.Items.Single(r => r.SubjectId == unassignedSubject.Id);
        Assert.Null(unassignedDto.TeacherId);
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
        {
            foreach (var row in Rows.Where(r => r.SectionId == sectionId))
            {
                row.Delete();
            }

            return Task.CompletedTask;
        }

        public Task SoftDeleteForSubjectAsync(Guid subjectId, CancellationToken ct = default)
        {
            foreach (var row in Rows.Where(r => r.SubjectId == subjectId))
            {
                row.Delete();
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeSectionRepository : ISectionRepository
    {
        public List<Section> Sections { get; } = new();

        public Task<List<Section>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Sections.ToList());

        public Task<Section?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Sections.FirstOrDefault(s => s.Id == id));

        public Task<List<Section>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => Task.FromResult(Sections.Where(s => ids.Contains(s.Id)).ToList());

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Sections.Any(s => s.Id == id));

        public Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
            => Task.FromResult(Sections.Any(s => s.Name == name && s.GradeId == gradeId));

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Section section, CancellationToken ct = default)
        {
            Sections.Add(section);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Section section, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSubjectRepository : ISubjectRepository
    {
        public List<Subject> Subjects { get; } = new();

        public Task<List<Subject>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Subjects.ToList());

        public Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Subjects.FirstOrDefault(s => s.Id == id));

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Subjects.Any(s => s.Id == id));

        public Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
            => Task.FromResult(Subjects.Any(s => s.Name == name && s.GradeId == gradeId));

        public Task<bool> HasAssignmentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Subject subject, CancellationToken ct = default)
        {
            Subjects.Add(subject);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Subject subject, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<AuthUser> Users { get; } = new();

        public Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

        public Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

        public Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.RefreshToken == refreshToken));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(Users.Any(u => u.Email == email));

        public Task<List<AuthUser>> GetByStatusAsync(AccountStatus status, CancellationToken ct = default)
            => Task.FromResult(Users.Where(u => u.Status == status).ToList());

        public Task<List<AuthUser>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Users.ToList());

        public Task<PagedResult<AuthUser>> GetPageAsync(
            int limit,
            DateTimeOffset? afterCreatedAt,
            Guid? afterId,
            AccountStatus? status,
            UserRole? role,
            CancellationToken ct = default)
            => Task.FromResult(PagedResult<AuthUser>.FromAll([]));

        public Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<int> CountUsableAdminsAsync(CancellationToken ct = default)
            => Task.FromResult(Users.Count(u => u.Role == UserRole.Admin && u.Status == AccountStatus.Approved && u.IsActive));

        public Task AddAsync(AuthUser user, CancellationToken ct = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AuthUser user, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
