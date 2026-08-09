using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class SectionSubject : BaseEntity
{
    public Guid SectionId { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid? TeacherId { get; private set; }

    public virtual Section? Section { get; private set; }
    public virtual Subject? Subject { get; private set; }
    public virtual AuthUser? Teacher { get; private set; }

    private SectionSubject()
    {
    }

    public void AssignTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    public void UnassignTeacher()
    {
        TeacherId = null;
    }

    public static SectionSubject Create(Guid sectionId, Guid subjectId, Guid? teacherId = null)
    {
        return new SectionSubject
        {
            SectionId = sectionId,
            SubjectId = subjectId,
            TeacherId = teacherId
        };
    }
}
