using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common;

public static class AssignmentGuards
{
    /// <summary>
    /// A teacher may only act on assignments they authored. This is a 403 rather than a 404
    /// because a teacher can legitimately learn that an assignment exists (an admin list, a
    /// colleague's link), so hiding it buys nothing and an honest refusal reads better.
    /// </summary>
    public static void EnsureOwnedBy(Assignment assignment, Guid teacherId)
    {
        if (assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("This assignment belongs to another teacher.");
        }
    }
}