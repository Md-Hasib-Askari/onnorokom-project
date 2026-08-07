using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class GradeService(IGradeRepository gradeRepository, IMapper mapper) : IGradeService
{
    public async Task<List<GradeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var grades = await gradeRepository.GetAllAsync(ct);
        return mapper.Map<List<GradeDto>>(grades);
    }

    public async Task<GradeDto> CreateAsync(GradeCreateRequest request, CancellationToken ct = default)
    {
        await EnsureUniqueAsync(request.Name, request.AcademicYear, null, ct);

        var grade = Grade.Create(request.Name, request.AcademicYear, request.Description);
        await gradeRepository.AddAsync(grade, ct);
        return mapper.Map<GradeDto>(grade);
    }

    public async Task<GradeDto> UpdateAsync(Guid id, GradeUpdateRequest request, CancellationToken ct = default)
    {
        var grade = await gradeRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Grade with id {id} was not found.");

        await EnsureUniqueAsync(request.Name, request.AcademicYear, grade, ct);

        grade.Update(request.Name, request.AcademicYear, request.Description);
        await gradeRepository.UpdateAsync(grade, ct);
        return mapper.Map<GradeDto>(grade);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await gradeRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Grade with id {id} was not found.");

        if (await gradeRepository.HasSubjectsAsync(id, ct))
        {
            throw new EntityInUseException($"Grade '{grade.Name}' cannot be deleted because it has subjects assigned.");
        }

        if (await gradeRepository.HasStudentsAsync(id, ct))
        {
            throw new EntityInUseException($"Grade '{grade.Name}' cannot be deleted because it has enrolled students.");
        }

        grade.Delete();
        await gradeRepository.UpdateAsync(grade, ct);
    }

    private async Task EnsureUniqueAsync(string name, string academicYear, Grade? exclude, CancellationToken ct)
    {
        var isSame = exclude is not null && exclude.Name == name && exclude.AcademicYear == academicYear;
        if (!isSame && await gradeRepository.ExistsAsync(name, academicYear, ct))
        {
            throw new DuplicateEntityException($"Grade '{name}' for academic year {academicYear} already exists.");
        }
    }
}
