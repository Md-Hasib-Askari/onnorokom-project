namespace AssignmentSystem.Application.DTOs.Subjects;

public record SubjectCreateRequest(string Name, Guid GradeId, string? Code = null);
