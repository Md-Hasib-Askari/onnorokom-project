namespace AssignmentSystem.Application.DTOs.Sections;

public record SectionDto(
    Guid Id,
    string Name,
    Guid GradeId,
    string? GradeName,
    int TeacherCount = 0,
    int StudentCount = 0);
