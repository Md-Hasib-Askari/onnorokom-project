using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Tests;

public class SubjectServiceTests
{
    private readonly FakeSubjectRepository _subjects = new();
    private readonly FakeGradeRepository _grades = new();
    private readonly FakeSectionSubjectRepository _sectionSubjects = new();
    private readonly SubjectService _sut;

    public SubjectServiceTests()
    {
        _sut = new SubjectService(_subjects, _grades, _sectionSubjects, TestMappers.CreateMapper());
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
    public async Task Update_MovedToAnotherGrade_SoftDeletesItsSectionTeacherLinks()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var otherGrade = Grade.Create("Grade 7", "2026");
        _grades.Grades.Add(grade);
        _grades.Grades.Add(otherGrade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);
        var link = SectionSubject.Create(Guid.NewGuid(), subject.Id, Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await _sut.UpdateAsync(subject.Id, new SubjectUpdateRequest("Mathematics", otherGrade.Id, Code: "MATH-6"));

        // Left live, the link ties the subject to a section of the grade it just left.
        Assert.True(link.IsDeleted);
    }

    [Fact]
    public async Task Update_SameGrade_LeavesSectionTeacherLinksIntact()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);
        var link = SectionSubject.Create(Guid.NewGuid(), subject.Id, Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await _sut.UpdateAsync(subject.Id, new SubjectUpdateRequest("Algebra", grade.Id, Code: "ALG-6"));

        Assert.False(link.IsDeleted);
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
    public async Task Delete_AlsoSoftDeletesItsSectionTeacherLinks()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var subject = Subject.Create("Mathematics", "MATH-6", grade.Id);
        _subjects.Subjects.Add(subject);
        var link = SectionSubject.Create(Guid.NewGuid(), subject.Id, Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await _sut.DeleteAsync(subject.Id);

        // Left live, the link keeps counting as "teacher still assigned" for a subject that is gone.
        Assert.True(link.IsDeleted);
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

        public Task<bool> HasSectionsAsync(Guid id, CancellationToken ct = default)
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
}
