using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Tests;

public class GradeServiceTests
{
    private readonly FakeGradeRepository _repo = new();
    private readonly GradeService _sut;

    public GradeServiceTests()
    {
        _sut = new GradeService(_repo, TestMappers.CreateMapper());
    }

    [Fact]
    public async Task Create_AddsGrade()
    {
        var dto = await _sut.CreateAsync(new GradeCreateRequest("Grade 6", "2026", "Sixth grade"));

        var grade = _repo.Grades.Single();
        Assert.Equal("Grade 6", grade.Name);
        Assert.Equal("2026", grade.AcademicYear);
        Assert.Equal("Sixth grade", grade.Description);
        Assert.Equal(grade.Id, dto.Id);
    }

    [Fact]
    public async Task Create_DuplicateNameAndYear_ThrowsDuplicateEntityException()
    {
        _repo.Grades.Add(Grade.Create("Grade 6", "2026"));

        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _sut.CreateAsync(new GradeCreateRequest("Grade 6", "2026")));
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);

        var dto = await _sut.UpdateAsync(grade.Id, new GradeUpdateRequest("Grade 7", "2027"));

        Assert.Equal("Grade 7", grade.Name);
        Assert.Equal("2027", grade.AcademicYear);
        Assert.Equal(grade.Id, dto.Id);
    }

    [Fact]
    public async Task Update_UnknownId_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), new GradeUpdateRequest("Grade 6", "2026")));
    }

    [Fact]
    public async Task Update_ChangingToExistingNameAndYear_ThrowsDuplicateEntityException()
    {
        var existing = Grade.Create("Grade 7", "2027");
        _repo.Grades.Add(existing);
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);

        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _sut.UpdateAsync(grade.Id, new GradeUpdateRequest("Grade 7", "2027")));
    }

    [Fact]
    public async Task Update_KeepingSameNameAndYear_DoesNotThrow()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);

        var dto = await _sut.UpdateAsync(grade.Id, new GradeUpdateRequest("Grade 6", "2026", "Updated"));

        Assert.Equal("Updated", grade.Description);
    }

    [Fact]
    public async Task Delete_MarksAsDeleted()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);

        await _sut.DeleteAsync(grade.Id);

        Assert.True(grade.IsDeleted);
    }

    [Fact]
    public async Task Delete_GradeWithSubjects_ThrowsEntityInUseException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);
        _repo.SubjectGradeIds.Add(grade.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(grade.Id));

        Assert.False(grade.IsDeleted);
    }

    [Fact]
    public async Task Delete_GradeWithSections_ThrowsEntityInUseException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);
        _repo.SectionGradeIds.Add(grade.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(grade.Id));

        Assert.False(grade.IsDeleted);
    }

    [Fact]
    public async Task Delete_GradeWithEnrolledStudents_ThrowsEntityInUseException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _repo.Grades.Add(grade);
        _repo.StudentGradeIds.Add(grade.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(grade.Id));

        Assert.False(grade.IsDeleted);
    }

    [Fact]
    public async Task Delete_UnknownId_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAll_ReturnsAllGradesAsAnUnpagedEnvelope()
    {
        _repo.Grades.Add(Grade.Create("Grade 6", "2026"));
        _repo.Grades.Add(Grade.Create("Grade 7", "2026"));

        var page = await _sut.GetAllAsync();

        Assert.Equal(2, page.Items.Count);
        Assert.False(page.HasMore);
        Assert.Null(page.NextCursor);
    }

    private sealed class FakeGradeRepository : IGradeRepository
    {
        public List<Grade> Grades { get; } = new();
        public List<Guid> SubjectGradeIds { get; } = new();
        public List<Guid> SectionGradeIds { get; } = new();
        public List<Guid> StudentGradeIds { get; } = new();

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
            => Task.FromResult(SubjectGradeIds.Contains(id));

        public Task<bool> HasSectionsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(SectionGradeIds.Contains(id));

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(StudentGradeIds.Contains(id));

        public Task AddAsync(Grade grade, CancellationToken ct = default)
        {
            Grades.Add(grade);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Grade grade, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
