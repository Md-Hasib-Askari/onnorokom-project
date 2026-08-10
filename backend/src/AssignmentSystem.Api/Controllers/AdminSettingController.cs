using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Settings;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminSettingController(ISystemSettingService systemSettingService) : ControllerBase
{
    [HttpGet("registration-policy")]
    public async Task<IActionResult> GetRegistrationPolicy(CancellationToken ct)
    {
        var policy = await systemSettingService.GetRegistrationPolicyAsync(ct);
        return Ok(policy);
    }

    [HttpPut("registration-policy")]
    public async Task<IActionResult> UpdateRegistrationPolicy(
        [FromBody] RegistrationPolicyUpdateRequest request,
        CancellationToken ct)
    {
        var policy = await systemSettingService.UpdateRegistrationPolicyAsync(request, ct);
        return Ok(policy);
    }
}
