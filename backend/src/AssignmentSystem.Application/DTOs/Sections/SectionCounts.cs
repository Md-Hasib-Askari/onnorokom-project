namespace AssignmentSystem.Application.DTOs.Sections;

/// <summary>
/// Counts per section for the admin section list. Teachers are distinct section-subject
/// teachers of the section; students are the profiles enrolled in it.
/// </summary>
public sealed record SectionCounts(int TeacherCount, int StudentCount);
