using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class SectionService(
    ISectionRepository sectionRepository,
    IGradeRepository gradeRepository,
    ISectionSubjectRepository sectionSubjectRepository,
    IMapper mapper) : ISectionService
{
    public async Task<PagedResult<SectionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var sections = await sectionRepository.GetAllAsync(ct);
        var counts = await sectionRepository.GetCountsAsync(ct);

        var items = sections
            .Select(section =>
            {
                var dto = mapper.Map<SectionDto>(section);
                return counts.TryGetValue(section.Id, out var count)
                    ? dto with { TeacherCount = count.TeacherCount, StudentCount = count.StudentCount }
                    : dto;
            })
            .ToList();

        return PagedResult<SectionDto>.FromAll(items);
    }

    public async Task<SectionDto> CreateAsync(SectionCreateRequest request, CancellationToken ct = default)
    {
        var grade = await GetGradeAsync(request.GradeId, ct);
        await EnsureNameUniqueAsync(request.Name, grade, null, ct);

        var section = Section.Create(request.Name, request.GradeId);
        await sectionRepository.AddAsync(section, ct);
        return mapper.Map<SectionDto>(section);
    }

    public async Task<SectionDto> UpdateAsync(Guid id, SectionUpdateRequest request, CancellationToken ct = default)
    {
        var section = await sectionRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Section with id {id} was not found.");

        var grade = await GetGradeAsync(request.GradeId, ct);
        await EnsureNameUniqueAsync(request.Name, grade, section, ct);

        var movedToAnotherGrade = section.GradeId != request.GradeId;
        if (movedToAnotherGrade)
        {
            // The links point at the old grade's subjects, which the section no longer teaches.
            // Left live they stay invisible in the subject list (it only lists the new grade's subjects)
            // yet still count as "teacher assigned", and reappear if the section moves back.
            await sectionSubjectRepository.SoftDeleteForSectionAsync(id, ct);
        }

        section.Update(request.Name, request.GradeId);
        await sectionRepository.UpdateAsync(section, ct);
        return mapper.Map<SectionDto>(section);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var section = await sectionRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Section with id {id} was not found.");

        if (await sectionRepository.HasStudentsAsync(id, ct))
        {
            throw new EntityInUseException($"Section '{section.Name}' cannot be deleted because it has enrolled students.");
        }

        await sectionSubjectRepository.SoftDeleteForSectionAsync(id, ct);

        section.Delete();
        await sectionRepository.UpdateAsync(section, ct);
    }

    private async Task<Grade> GetGradeAsync(Guid gradeId, CancellationToken ct)
    {
        return await gradeRepository.GetByIdAsync(gradeId, ct)
            ?? throw new EntityNotFoundException($"Grade with id {gradeId} was not found.");
    }

    private async Task EnsureNameUniqueAsync(string name, Grade grade, Section? exclude, CancellationToken ct)
    {
        var isSame = exclude is not null && exclude.Name == name && exclude.GradeId == grade.Id;
        if (!isSame && await sectionRepository.ExistsByNameAsync(name, grade.Id, ct))
        {
            throw new DuplicateEntityException($"Section '{name}' in {grade.Name} already exists.");
        }
    }
}
