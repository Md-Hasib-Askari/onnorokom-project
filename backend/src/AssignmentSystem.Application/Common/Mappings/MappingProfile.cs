using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuthUser, UserListItemDto>();
        CreateMap<Grade, GradeDto>();
        CreateMap<Subject, SubjectDto>()
            .ForCtorParam(nameof(SubjectDto.TeacherName), o => o.MapFrom(s => s.Teacher != null ? s.Teacher.FullName : null));
        CreateMap<Assignment, AssignmentListItemDto>()
            .ForCtorParam(nameof(AssignmentListItemDto.GradeName), o => o.MapFrom(s => s.Subject != null && s.Subject.Grade != null ? s.Subject.Grade.Name : null))
            .ForCtorParam(nameof(AssignmentListItemDto.TeacherName), o => o.MapFrom(s => s.Teacher != null ? s.Teacher.FullName : null));
        CreateMap<Submission, SubmissionListItemDto>()
            .ForCtorParam(nameof(SubmissionListItemDto.StudentName), o => o.MapFrom(s => s.Student != null ? s.Student.FullName : null));
    }
}
