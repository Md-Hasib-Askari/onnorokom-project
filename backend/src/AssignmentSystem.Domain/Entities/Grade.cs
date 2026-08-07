using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Grade : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string AcademicYear { get; private set; } = null!;

    private Grade()
    {
    }

    public static Grade Create(string name, string academicYear, string? description = null)
    {
        return new Grade
        {
            Name = name,
            AcademicYear = academicYear,
            Description = description
        };
    }
}
