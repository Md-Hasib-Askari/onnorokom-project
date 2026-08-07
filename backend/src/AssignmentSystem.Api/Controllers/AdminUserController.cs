using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminUserController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminUserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken ct)
    {
        var users = await _authService.GetPendingUsersAsync(ct);
        return Ok(users);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveUser([FromBody] ApproveUserRequest request, CancellationToken ct)
    {
        var user = await _authService.ApproveAsync(request.UserId, request.Approve, ct);
        return Ok(new { user.Id, user.Email, user.FullName, user.Role, user.Status });
    }
}
