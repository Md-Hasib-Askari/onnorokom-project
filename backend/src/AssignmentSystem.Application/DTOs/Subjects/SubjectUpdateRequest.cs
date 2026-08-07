namespace AssignmentSystem.Application.DTOs.Subjects;

public record SubjectUpdateRequest(string Name, string Code, Guid GradeId);
