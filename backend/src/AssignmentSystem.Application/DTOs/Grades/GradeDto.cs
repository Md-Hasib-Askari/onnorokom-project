namespace AssignmentSystem.Application.DTOs.Grades;

public record GradeDto(
    Guid Id,
    string Name,
    string AcademicYear,
    string? Description,
    int TeacherCount = 0,
    int StudentCount = 0);
