namespace AssignmentSystem.Application.DTOs.Sections;

public record SectionSubjectDto(
    Guid SubjectId,
    string SubjectName,
    string? SubjectCode,
    Guid? TeacherId,
    string? TeacherName);
