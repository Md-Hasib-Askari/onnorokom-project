namespace AssignmentSystem.Application.DTOs.Sections;

public record SectionDto(Guid Id, string Name, Guid GradeId, string? GradeName);
