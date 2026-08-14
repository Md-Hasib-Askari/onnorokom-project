using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Services;

public class SectionSubjectService(
    ISectionSubjectRepository sectionSubjectRepository,
    ISectionRepository sectionRepository,
    ISubjectRepository subjectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ISectionSubjectService
{
    public async Task<PagedResult<SectionSubjectDto>> GetSectionSubjectsAsync(Guid sectionId, CancellationToken ct = default)
    {
        var section = await sectionRepository.GetByIdAsync(sectionId, ct)
            ?? throw new EntityNotFoundException($"Section with id {sectionId} was not found.");

        var gradeSubjects = (await subjectRepository.GetAllAsync(ct))
            .Where(s => s.GradeId == section.GradeId)
            .OrderBy(s => s.Name)
            .ToList();

        var assignments = await sectionSubjectRepository.GetBySectionAsync(sectionId, ct);
        var assignmentBySubjectId = assignments.ToDictionary(a => a.SubjectId);

        var items = gradeSubjects.Select(subject =>
        {
            var assignment = assignmentBySubjectId.GetValueOrDefault(subject.Id);
            return new SectionSubjectDto(
                subject.Id,
                subject.Name,
                subject.Code,
                assignment?.TeacherId,
                assignment?.Teacher?.FullName);
        }).ToList();

        return PagedResult<SectionSubjectDto>.FromAll(items);
    }

    public async Task<SectionSubjectDto> AssignTeacherAsync(Guid sectionId, Guid subjectId, Guid teacherId, CancellationToken ct = default)
    {
        var (section, subject) = await EnsureSectionAndSubjectMatchAsync(sectionId, subjectId, ct);
        await TeacherEligibilityGuard.EnsureIsTeacherAsync(userRepository, teacherId, ct);

        var sectionSubject = await sectionSubjectRepository.GetBySectionAndSubjectAsync(sectionId, subjectId, ct);
        if (sectionSubject is null)
        {
            sectionSubject = SectionSubject.Create(sectionId, subjectId, teacherId);
            await sectionSubjectRepository.AddAsync(sectionSubject, ct);
        }
        else
        {
            sectionSubject.AssignTeacher(teacherId);
            await sectionSubjectRepository.UpdateAsync(sectionSubject, ct);
        }

        await unitOfWork.SaveAsync(ct);

        var teacher = await userRepository.GetByIdAsync(teacherId, ct);
        return new SectionSubjectDto(subject.Id, subject.Name, subject.Code, teacherId, teacher?.FullName);
    }

    public async Task<SectionSubjectDto> UnassignTeacherAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default)
    {
        var (_, subject) = await EnsureSectionAndSubjectMatchAsync(sectionId, subjectId, ct);

        var sectionSubject = await sectionSubjectRepository.GetBySectionAndSubjectAsync(sectionId, subjectId, ct);
        if (sectionSubject is not null)
        {
            sectionSubject.UnassignTeacher();
            await sectionSubjectRepository.UpdateAsync(sectionSubject, ct);
            await unitOfWork.SaveAsync(ct);
        }

        return new SectionSubjectDto(subject.Id, subject.Name, subject.Code, null, null);
    }

    private async Task<(Section Section, Domain.Entities.Subject Subject)> EnsureSectionAndSubjectMatchAsync(Guid sectionId, Guid subjectId, CancellationToken ct)
    {
        var section = await sectionRepository.GetByIdAsync(sectionId, ct)
            ?? throw new EntityNotFoundException($"Section with id {sectionId} was not found.");

        var subject = await subjectRepository.GetByIdAsync(subjectId, ct)
            ?? throw new EntityNotFoundException($"Subject with id {subjectId} was not found.");

        if (subject.GradeId != section.GradeId)
        {
            throw new DomainException($"Subject '{subject.Name}' does not belong to this section's grade.");
        }

        return (section, subject);
    }
}
