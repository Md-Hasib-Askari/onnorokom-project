using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminUserController(
    IAuthService authService,
    IAdminUserService adminUserService,
    IValidator<UserCreateRequest> createValidator,
    IValidator<UserUpdateRequest> updateValidator) : ControllerBase
{
    [HttpPost("approve")]
    public async Task<IActionResult> ApproveUser([FromBody] ApproveUserRequest request, CancellationToken ct)
    {
        var user = await authService.ApproveAsync(request.UserId, request.Approve, request.StudentSectionId, ct);
        return Ok(new ApproveUserResponse(user.Id, user.Email, user.FullName, user.Role, user.Status));
    }

    /// <summary>
    /// Paginated, keyset-ordered by <c>(CreatedAt, Id)</c> ascending. The pending list is a
    /// <c>status=Pending</c> filter on this same endpoint; <c>role</c> is accepted for the
    /// section-subjects teacher picker (<c>?role=Teacher&amp;limit=100</c>).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        [FromQuery] AccountStatus? status,
        [FromQuery] UserRole? role,
        CancellationToken ct)
    {
        var users = await adminUserService.GetAllUsersAsync(new PageRequest(limit), cursor, status, role, ct);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var user = await adminUserService.GetUserByIdAsync(id, ct);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var user = await adminUserService.CreateUserAsync(request, ct);
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var user = await adminUserService.UpdateUserAsync(id, request, ct);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await adminUserService.DeleteUserAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken ct)
    {
        await adminUserService.ResetPasswordAsync(id, ct);
        return NoContent();
    }
}
