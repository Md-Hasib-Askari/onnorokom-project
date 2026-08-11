using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Grade, GradeDto>();
        CreateMap<Subject, SubjectDto>();
        CreateMap<Section, SectionDto>()
            .ForCtorParam(nameof(SectionDto.GradeName), o => o.MapFrom(s => s.Grade != null ? s.Grade.Name : null));
        CreateMap<Submission, SubmissionListItemDto>()
            .ForCtorParam(nameof(SubmissionListItemDto.StudentName), o => o.MapFrom(s => s.Student != null ? s.Student.FullName : null));
    }
}
