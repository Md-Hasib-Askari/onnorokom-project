using AssignmentSystem.Application.Common.Interfaces;
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
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken ct)
    {
        var users = await authService.GetPendingUsersAsync(ct);
        return Ok(users);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveUser([FromBody] ApproveUserRequest request, CancellationToken ct)
    {
        var user = await authService.ApproveAsync(request.UserId, request.Approve, request.StudentSectionId, ct);
        return Ok(new ApproveUserResponse(user.Id, user.Email, user.FullName, user.Role, user.Status));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var users = await adminUserService.GetAllUsersAsync(ct);
        return Ok(users);
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
}
