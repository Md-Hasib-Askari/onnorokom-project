namespace AssignmentSystem.Application.DTOs.Grades;

public record GradeCreateRequest(string Name, string AcademicYear, string? Description = null);
