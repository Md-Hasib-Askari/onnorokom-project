using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class SubjectServiceTests
{
    private readonly FakeSubjectRepository _subjects = new();
    private readonly FakeGradeRepository _grades = new();
    private readonly FakeUserRepository _users = new();
    private readonly SubjectService _sut;

    public SubjectServiceTests()
    {
        _sut = new SubjectService(_subjects, _grades, _users, TestMappers.CreateMapper());
    }

    [Fact]
    public async Task Create_AddsSubject()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);

        var dto = await _sut.CreateAsync(new SubjectCreateRequest("Mathematics", grade.Id, Code: "MATH-6"));

        var subject = _subjects.Subjects.Single();
        Assert.Equal("Mathematics", subject.Name);
        Assert.Equal("MATH-6", subject.Code);
        Assert.Equal(grade.Id, subject.GradeId);
        Assert.Null(subject.TeacherId);
        Assert.Equal(subject.Id, dto.Id);
    }

    [Fact]
    public async Task Create_WithoutCode_AddsSubjectWithNullCode()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);

        var dto = await _sut.CreateAsync(new SubjectCreateRequest("Mathematics", grade.Id));

        var subject = _subjects.Subjects.Single();
        Assert.Null(subject.Code);
        Assert.Equal(subject.Id, dto.Id);
    }

    [Fact]
    public async Task Create_WithTeacher_AssignsTeacher()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var teacher = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        teacher.Approve();
        _users.Users.Add(teacher);

        var dto = await _sut.CreateAsync(new SubjectCreateRequest("Mathematics", grade.Id, teacher.Id, Code: "MATH-6"));

        Assert.Equal(teacher.Id, dto.TeacherId);
    }

    [Fact]
    public async Task Create_UnknownGrade_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.CreateAsync(new SubjectCreateRequest("Mathematics", Guid.NewGuid())));
    }

    [Fact]
    public async Task Create_DuplicateNameInGrade_ThrowsDuplicateEntityException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        _subjects.Subjects.Add(Subject.Create("Mathematics", null, grade.Id));

        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _sut.CreateAsync(new SubjectCreateRequest("Mathematics", grade.Id)));
    }

    [Fact]
    public async Task Create_SameNameInDifferentGrade_DoesNotThrow()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var otherGrade = Grade.Create("Grade 7", "2026");
        _grades.Grades.Add(grade);
        _grades.Grades.Add(otherGrade);
        _subjects.Subjects.Add(Subject.Create("Mathematics", null, grade.Id));

        var dto = await _sut.CreateAsync(new SubjectCreateRequest("Mathematics", otherGrade.Id));

        Assert.Equal(otherGrade.Id, dto.GradeId);
    }

    [Fact]
    public async Task Create_WithNonTeacherUser_ThrowsInvalidTeacherException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var student = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        student.Approve();
        _users.Users.Add(student);

        await Assert.ThrowsAsync<InvalidTeacherException>(
            () => _sut.CreateAsync(new SubjectCreateRequest("Mathematics", grade.Id, student.Id)));
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);

        var dto = await _sut.UpdateAsync(subject.Id, new SubjectUpdateRequest("Algebra", grade.Id, Code: "ALG-6"));

        Assert.Equal("Algebra", subject.Name);
        Assert.Equal("ALG-6", subject.Code);
        Assert.Equal(subject.Id, dto.Id);
    }

    [Fact]
    public async Task Update_WithoutCode_ClearsExistingCode()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);

        var dto = await _sut.UpdateAsync(subject.Id, new SubjectUpdateRequest("Mathematics", grade.Id));

        Assert.Null(subject.Code);
        Assert.Equal(subject.Id, dto.Id);
    }

    [Fact]
    public async Task Update_UnknownId_ThrowsEntityNotFoundException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), new SubjectUpdateRequest("Algebra", grade.Id)));
    }

    [Fact]
    public async Task Delete_MarksAsDeleted()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);

        await _sut.DeleteAsync(subject.Id);

        Assert.True(subject.IsDeleted);
    }

    [Fact]
    public async Task Delete_SubjectWithAssignments_ThrowsEntityInUseException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);
        _subjects.AssignmentSubjectIds.Add(subject.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(subject.Id));

        Assert.False(subject.IsDeleted);
    }

    [Fact]
    public async Task AssignTeacher_SetsTeacher()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var teacher = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        teacher.Approve();
        _users.Users.Add(teacher);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);

        var dto = await _sut.AssignTeacherAsync(subject.Id, teacher.Id);

        Assert.Equal(teacher.Id, subject.TeacherId);
        Assert.Equal(teacher.Id, dto.TeacherId);
    }

    [Fact]
    public async Task AssignTeacher_NonTeacher_ThrowsInvalidTeacherException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var student = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        student.Approve();
        _users.Users.Add(student);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);

        await Assert.ThrowsAsync<InvalidTeacherException>(
            () => _sut.AssignTeacherAsync(subject.Id, student.Id));
    }

    [Fact]
    public async Task AssignTeacher_PendingTeacher_ThrowsInvalidTeacherException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var teacher = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        _users.Users.Add(teacher);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);

        await Assert.ThrowsAsync<InvalidTeacherException>(
            () => _sut.AssignTeacherAsync(subject.Id, teacher.Id));
    }

    [Fact]
    public async Task AssignTeacher_UnknownSubject_ThrowsEntityNotFoundException()
    {
        var teacher = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        teacher.Approve();
        _users.Users.Add(teacher);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.AssignTeacherAsync(Guid.NewGuid(), teacher.Id));
    }

    [Fact]
    public async Task UnassignTeacher_ClearsTeacher()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id, teacherId: Guid.NewGuid());
        _subjects.Subjects.Add(subject);

        var dto = await _sut.UnassignTeacherAsync(subject.Id);

        Assert.Null(subject.TeacherId);
        Assert.Null(dto.TeacherId);
    }

    private sealed class FakeSubjectRepository : ISubjectRepository
    {
        public List<Subject> Subjects { get; } = new();
        public List<Guid> AssignmentSubjectIds { get; } = new();

        public Task<List<Subject>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Subjects.ToList());

        public Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Subjects.FirstOrDefault(s => s.Id == id));

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Subjects.Any(s => s.Id == id));

        public Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
            => Task.FromResult(Subjects.Any(s => s.Name == name && s.GradeId == gradeId));

        public Task<bool> HasAssignmentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(AssignmentSubjectIds.Contains(id));

        public Task AddAsync(Subject subject, CancellationToken ct = default)
        {
            Subjects.Add(subject);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Subject subject, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeGradeRepository : IGradeRepository
    {
        public List<Grade> Grades { get; } = new();

        public Task<List<Grade>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Grades.ToList());

        public Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Grades.FirstOrDefault(g => g.Id == id));

        public Task<List<Grade>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => Task.FromResult(Grades.Where(g => ids.Contains(g.Id)).ToList());

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Grades.Any(g => g.Id == id));

        public Task<bool> ExistsAsync(string name, string academicYear, CancellationToken ct = default)
            => Task.FromResult(Grades.Any(g => g.Name == name && g.AcademicYear == academicYear));

        public Task<bool> HasSubjectsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Grade grade, CancellationToken ct = default)
        {
            Grades.Add(grade);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Grade grade, CancellationToken ct = default)
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
