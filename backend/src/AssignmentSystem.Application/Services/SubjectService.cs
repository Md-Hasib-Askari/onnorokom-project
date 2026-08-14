using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class SubjectService(
    ISubjectRepository subjectRepository,
    IGradeRepository gradeRepository,
    ISectionSubjectRepository sectionSubjectRepository,
    IMapper mapper) : ISubjectService
{
    public async Task<PagedResult<SubjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        var subjects = await subjectRepository.GetAllAsync(ct);
        var teacherCounts = await subjectRepository.GetTeacherCountsAsync(ct);

        var items = subjects
            .Select(subject =>
            {
                var dto = mapper.Map<SubjectDto>(subject);
                return teacherCounts.TryGetValue(subject.Id, out var count)
                    ? dto with { TeacherCount = count }
                    : dto;
            })
            .ToList();

        return PagedResult<SubjectDto>.FromAll(items);
    }

    public async Task<SubjectDto> CreateAsync(SubjectCreateRequest request, CancellationToken ct = default)
    {
        var grade = await GetGradeAsync(request.GradeId, ct);
        await EnsureNameUniqueAsync(request.Name, grade, null, ct);

        var subject = Subject.Create(request.Name, request.Code, request.GradeId);
        await subjectRepository.AddAsync(subject, ct);
        return mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto> UpdateAsync(Guid id, SubjectUpdateRequest request, CancellationToken ct = default)
    {
        var subject = await subjectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Subject with id {id} was not found.");

        var grade = await GetGradeAsync(request.GradeId, ct);
        await EnsureNameUniqueAsync(request.Name, grade, subject, ct);

        var movedToAnotherGrade = subject.GradeId != request.GradeId;
        if (movedToAnotherGrade)
        {
            // The links tie this subject to sections of its old grade, a pairing the subject
            // list rejects outright. Left live they stay invisible yet still count as "teacher
            // assigned", and reappear if the subject moves back.
            await sectionSubjectRepository.SoftDeleteForSubjectAsync(id, ct);
        }

        subject.Update(request.Name, request.Code, request.GradeId);
        await subjectRepository.UpdateAsync(subject, ct);
        return mapper.Map<SubjectDto>(subject);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await subjectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Subject with id {id} was not found.");

        if (await subjectRepository.HasAssignmentsAsync(id, ct))
        {
            throw new EntityInUseException($"Subject '{subject.Name}' cannot be deleted because it has assignments.");
        }

        await sectionSubjectRepository.SoftDeleteForSubjectAsync(id, ct);

        subject.Delete();
        await subjectRepository.UpdateAsync(subject, ct);
    }

    private async Task<Grade> GetGradeAsync(Guid gradeId, CancellationToken ct)
    {
        return await gradeRepository.GetByIdAsync(gradeId, ct)
            ?? throw new EntityNotFoundException($"Grade with id {gradeId} was not found.");
    }

    private async Task EnsureNameUniqueAsync(string name, Grade grade, Subject? exclude, CancellationToken ct)
    {
        var isSame = exclude is not null && exclude.Name == name && exclude.GradeId == grade.Id;
        if (!isSame && await subjectRepository.ExistsByNameAsync(name, grade.Id, ct))
        {
            throw new DuplicateEntityException($"Subject '{name}' in {grade.Name} already exists.");
        }
    }
}
