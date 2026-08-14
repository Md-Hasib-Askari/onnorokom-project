namespace AssignmentSystem.Application.DTOs.Grades;

/// <summary>
/// Counts per grade for the admin grade list. Teachers are distinct section-subject teachers in
/// the grade's sections; students are profiles enrolled in those sections.
/// </summary>
public sealed record GradeCounts(int TeacherCount, int StudentCount);
