using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Subject : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public Guid GradeId { get; private set; }
    public Guid? TeacherId { get; private set; }

    public virtual Grade? Grade { get; private set; }
    public virtual AuthUser? Teacher { get; private set; }

    private Subject()
    {
    }

    public void Update(string name, string code, Guid gradeId)
    {
        Name = name;
        Code = code;
        GradeId = gradeId;
    }

    public void AssignTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    public void UnassignTeacher()
    {
        TeacherId = null;
    }

    public static Subject Create(string name, string code, Guid gradeId, Guid? teacherId = null)
    {
        return new Subject
        {
            Name = name,
            Code = code,
            GradeId = gradeId,
            TeacherId = teacherId
        };
    }
}
