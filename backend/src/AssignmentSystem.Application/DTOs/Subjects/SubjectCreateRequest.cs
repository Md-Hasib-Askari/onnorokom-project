namespace AssignmentSystem.Application.DTOs.Subjects;

public record SubjectCreateRequest(string Name, string Code, Guid GradeId, Guid? TeacherId = null);
