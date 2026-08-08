namespace AssignmentSystem.Application.DTOs.Subjects;

public record SubjectUpdateRequest(string Name, Guid GradeId, string? Code = null);
