namespace AssignmentSystem.Application.DTOs.Subjects;

public record SubjectDto(
    Guid Id,
    string Name,
    string? Code,
    Guid GradeId,
    string? GradeName);
