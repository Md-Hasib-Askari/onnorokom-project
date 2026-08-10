using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Subject : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Code { get; private set; }
    public Guid GradeId { get; private set; }

    public virtual Grade? Grade { get; private set; }

    private Subject()
    {
    }

    public void Update(string name, string? code, Guid gradeId)
    {
        Name = name;
        Code = code;
        GradeId = gradeId;
    }

    public static Subject Create(string name, string? code, Guid gradeId)
    {
        return new Subject
        {
            Name = name,
            Code = code,
            GradeId = gradeId
        };
    }
}
