using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class SubjectService(
    ISubjectRepository subjectRepository,
    IGradeRepository gradeRepository,
    IUserRepository userRepository,
    IMapper mapper) : ISubjectService
{
    public async Task<List<SubjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        var subjects = await subjectRepository.GetAllAsync(ct);
        return mapper.Map<List<SubjectDto>>(subjects);
    }

    public async Task<SubjectDto> CreateAsync(SubjectCreateRequest request, CancellationToken ct = default)
    {
        await EnsureGradeExistsAsync(request.GradeId, ct);
        await EnsureCodeUniqueAsync(request.Code, request.GradeId, null, ct);
        await EnsureIsTeacherAsync(request.TeacherId, ct);

        var subject = Subject.Create(request.Name, request.Code, request.GradeId, request.TeacherId);
        await subjectRepository.AddAsync(subject, ct);
        return mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto> UpdateAsync(Guid id, SubjectUpdateRequest request, CancellationToken ct = default)
    {
        var subject = await subjectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Subject with id {id} was not found.");

        await EnsureGradeExistsAsync(request.GradeId, ct);
        await EnsureCodeUniqueAsync(request.Code, request.GradeId, subject, ct);

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

        subject.Delete();
        await subjectRepository.UpdateAsync(subject, ct);
    }

    public async Task<SubjectDto> AssignTeacherAsync(Guid id, Guid teacherId, CancellationToken ct = default)
    {
        var subject = await subjectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Subject with id {id} was not found.");

        await EnsureIsTeacherAsync(teacherId, ct);

        subject.AssignTeacher(teacherId);
        await subjectRepository.UpdateAsync(subject, ct);
        return mapper.Map<SubjectDto>(subject);
    }

    public async Task<SubjectDto> UnassignTeacherAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await subjectRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"Subject with id {id} was not found.");

        subject.UnassignTeacher();
        await subjectRepository.UpdateAsync(subject, ct);
        return mapper.Map<SubjectDto>(subject);
    }

    private async Task EnsureGradeExistsAsync(Guid gradeId, CancellationToken ct)
    {
        if (!await gradeRepository.ExistsAsync(gradeId, ct))
        {
            throw new EntityNotFoundException($"Grade with id {gradeId} was not found.");
        }
    }

    private async Task EnsureCodeUniqueAsync(string code, Guid gradeId, Subject? exclude, CancellationToken ct)
    {
        var isSame = exclude is not null && exclude.Code == code && exclude.GradeId == gradeId;
        if (!isSame && await subjectRepository.ExistsAsync(code, gradeId, ct))
        {
            throw new DuplicateEntityException($"Subject with code '{code}' in grade {gradeId} already exists.");
        }
    }

    private async Task EnsureIsTeacherAsync(Guid? teacherId, CancellationToken ct)
    {
        if (teacherId is null)
        {
            return;
        }

        var teacher = await userRepository.GetByIdAsync(teacherId.Value, ct);
        if (teacher is null || teacher.Role != UserRole.Teacher || teacher.Status != AccountStatus.Approved || !teacher.IsActive)
        {
            throw new InvalidTeacherException($"User with id {teacherId} is not an approved active teacher.");
        }
    }
}
