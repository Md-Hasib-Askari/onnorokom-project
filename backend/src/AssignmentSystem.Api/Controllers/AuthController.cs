using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ISystemSettingService systemSettingService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshTokenValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator) : ControllerBase
{
    /// <summary>
    /// Lets the sign-up page show only the roles an admin has opened. Anonymous by necessity, and
    /// safe because it returns two policy booleans rather than the settings store.
    /// </summary>
    [HttpGet("registration-policy")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRegistrationPolicy(CancellationToken ct)
    {
        var policy = await systemSettingService.GetRegistrationPolicyAsync(ct);
        return Ok(policy);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var validation = await registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var user = await authService.RegisterAsync(request, ct);
        return Ok(new RegisterResponse(user.Id, user.Email, user.FullName, user.Role, user.Status));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var validation = await loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var response = await authService.LoginAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var validation = await refreshTokenValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var response = await authService.RefreshAsync(request.RefreshToken, ct);
        return Ok(response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var validation = await refreshTokenValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        await authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var validation = await forgotPasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        await authService.ForgotPasswordAsync(request.Email, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var validation = await resetPasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        await authService.ResetPasswordAsync(request, ct);
        return NoContent();
    }
}
