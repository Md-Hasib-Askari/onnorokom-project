using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminUserController(IAuthService authService) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken ct)
    {
        var users = await authService.GetPendingUsersAsync(ct);
        return Ok(users);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveUser([FromBody] ApproveUserRequest request, CancellationToken ct)
    {
        var user = await authService.ApproveAsync(request.UserId, request.Approve, ct);
        return Ok(new { user.Id, user.Email, user.FullName, user.Role, user.Status });
    }
}
