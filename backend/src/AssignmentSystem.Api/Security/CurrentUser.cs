using System.Security.Claims;
using AssignmentSystem.Application.Common.Interfaces;

namespace AssignmentSystem.Api.Security;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
