namespace AssignmentSystem.Application.DTOs.Teacher;

/// <summary>
/// One section-subject pairing the signed-in teacher holds. This is the full set of targets they
/// may create an assignment against.
/// </summary>
public record TeacherSectionSubjectDto(
    Guid SectionId,
    string? SectionName,
    Guid GradeId,
    string? GradeName,
    Guid SubjectId,
    string? SubjectName,
    string? SubjectCode);