namespace AssignmentSystem.Application.DTOs.Grades;

public record GradeUpdateRequest(string Name, string AcademicYear, string? Description = null);
