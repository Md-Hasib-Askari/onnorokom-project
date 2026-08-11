using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;

namespace AssignmentSystem.Application.Common;

public static class CurrentUserExtensions
{
    /// <summary>
    /// The caller's id, for the teacher and student services that scope every read and write to
    /// the signed-in user. A missing or malformed subject claim means the token is not one this
    /// API issued, so it is treated as a refusal rather than a 500.
    /// </summary>
    public static Guid GetRequiredUserId(this ICurrentUser currentUser)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
        {
            throw new ForbiddenException("The request is not associated with a valid user account.");
        }

        return userId;
    }
}