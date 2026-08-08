namespace AssignmentSystem.Application.DTOs.Subjects;

public record SubjectCreateRequest(string Name, Guid GradeId, Guid? TeacherId = null, string? Code = null);
