using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Tests;

public class SectionServiceTests
{
    private readonly FakeSectionRepository _repo = new();
    private readonly FakeGradeRepository _grades = new();
    private readonly FakeSectionSubjectRepository _sectionSubjects = new();
    private readonly SectionService _sut;

    public SectionServiceTests()
    {
        _sut = new SectionService(_repo, _grades, _sectionSubjects, TestMappers.CreateMapper());
    }

    [Fact]
    public async Task Create_AddsSection()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);

        var dto = await _sut.CreateAsync(new SectionCreateRequest("Section A", grade.Id));

        var section = _repo.Sections.Single();
        Assert.Equal("Section A", section.Name);
        Assert.Equal(grade.Id, section.GradeId);
        Assert.Equal(section.Id, dto.Id);
    }

    [Fact]
    public async Task Create_UnknownGrade_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.CreateAsync(new SectionCreateRequest("Section A", Guid.NewGuid())));
    }

    [Fact]
    public async Task Create_DuplicateNameInGrade_ThrowsDuplicateEntityException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        _repo.Sections.Add(Section.Create("Section A", grade.Id));

        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _sut.CreateAsync(new SectionCreateRequest("Section A", grade.Id)));
    }

    [Fact]
    public async Task Create_SameNameInDifferentGrade_DoesNotThrow()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var otherGrade = Grade.Create("Grade 7", "2026");
        _grades.Grades.Add(grade);
        _grades.Grades.Add(otherGrade);
        _repo.Sections.Add(Section.Create("Section A", grade.Id));

        var dto = await _sut.CreateAsync(new SectionCreateRequest("Section A", otherGrade.Id));

        Assert.Equal(otherGrade.Id, dto.GradeId);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);

        var dto = await _sut.UpdateAsync(section.Id, new SectionUpdateRequest("Section B", grade.Id));

        Assert.Equal("Section B", section.Name);
        Assert.Equal(section.Id, dto.Id);
    }

    [Fact]
    public async Task Update_MovedToAnotherGrade_SoftDeletesItsSubjectTeacherLinks()
    {
        var grade = Grade.Create("Grade 6", "2026");
        var otherGrade = Grade.Create("Grade 7", "2026");
        _grades.Grades.Add(grade);
        _grades.Grades.Add(otherGrade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);
        var link = SectionSubject.Create(section.Id, Guid.NewGuid(), Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await _sut.UpdateAsync(section.Id, new SectionUpdateRequest("Section A", otherGrade.Id));

        // Left live, the link keeps a teacher on a subject the section's new grade does not teach.
        Assert.True(link.IsDeleted);
    }

    [Fact]
    public async Task Update_SameGrade_LeavesSubjectTeacherLinksIntact()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);
        var link = SectionSubject.Create(section.Id, Guid.NewGuid(), Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await _sut.UpdateAsync(section.Id, new SectionUpdateRequest("Section B", grade.Id));

        Assert.False(link.IsDeleted);
    }

    [Fact]
    public async Task Update_UnknownId_ThrowsEntityNotFoundException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), new SectionUpdateRequest("Section A", grade.Id)));
    }

    [Fact]
    public async Task Delete_MarksAsDeleted()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);

        await _sut.DeleteAsync(section.Id);

        Assert.True(section.IsDeleted);
    }

    [Fact]
    public async Task Delete_AlsoSoftDeletesItsSubjectTeacherLinks()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);
        var link = SectionSubject.Create(section.Id, Guid.NewGuid(), Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await _sut.DeleteAsync(section.Id);

        // Left live, the link keeps counting as "teacher still assigned" for a section that is gone.
        Assert.True(link.IsDeleted);
    }

    [Fact]
    public async Task Delete_SectionWithStudents_LeavesSubjectTeacherLinksIntact()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);
        _repo.StudentSectionIds.Add(section.Id);
        var link = SectionSubject.Create(section.Id, Guid.NewGuid(), Guid.NewGuid());
        _sectionSubjects.Rows.Add(link);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(section.Id));

        Assert.False(link.IsDeleted);
    }

    [Fact]
    public async Task Delete_SectionWithStudents_ThrowsEntityInUseException()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var section = Section.Create("Section A", grade.Id);
        _repo.Sections.Add(section);
        _repo.StudentSectionIds.Add(section.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteAsync(section.Id));

        Assert.False(section.IsDeleted);
    }

    [Fact]
    public async Task Delete_UnknownId_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAll_ReturnsAllSections()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        _repo.Sections.Add(Section.Create("Section A", grade.Id));
        _repo.Sections.Add(Section.Create("Section B", grade.Id));

        var sections = await _sut.GetAllAsync();

        Assert.Equal(2, sections.Count);
    }

    private sealed class FakeSectionRepository : ISectionRepository
    {
        public List<Section> Sections { get; } = new();
        public List<Guid> StudentSectionIds { get; } = new();

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
            => Task.FromResult(StudentSectionIds.Contains(id));

        public Task AddAsync(Section section, CancellationToken ct = default)
        {
            Sections.Add(section);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Section section, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeSectionSubjectRepository : ISectionSubjectRepository
    {
        public List<SectionSubject> Rows { get; } = new();

        public Task<List<SectionSubject>> GetBySectionAsync(Guid sectionId, CancellationToken ct = default)
            => Task.FromResult(Rows.Where(r => r.SectionId == sectionId).ToList());

        public Task<SectionSubject?> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default)
            => Task.FromResult(Rows.FirstOrDefault(r => r.SectionId == sectionId && r.SubjectId == subjectId));

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
