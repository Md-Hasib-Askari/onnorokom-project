using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Section : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Guid GradeId { get; private set; }

    public virtual Grade? Grade { get; private set; }

    private Section()
    {
    }

    public void Update(string name, Guid gradeId)
    {
        Name = name;
        GradeId = gradeId;
    }

    public static Section Create(string name, Guid gradeId)
    {
        return new Section
        {
            Name = name,
            GradeId = gradeId
        };
    }
}
