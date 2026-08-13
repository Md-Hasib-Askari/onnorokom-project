namespace AssignmentSystem.Application.DTOs.Teacher;

/// <summary>
/// One student in a section the signed-in teacher teaches. A student who takes two subjects from
/// the same teacher still appears once, since this describes class membership, not a subject link.
/// </summary>
public record TeacherStudentDto(
    Guid Id,
    string FullName,
    string? RollNumber,
    Guid SectionId,
    string? SectionName,
    string? GradeName);
