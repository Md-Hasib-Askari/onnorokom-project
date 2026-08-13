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
    [HttpGet]
    public async Task<IActionResult> GetSystemSettings(CancellationToken ct)
    {
        var settings = await systemSettingService.GetSystemSettingsAsync(ct);
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSystemSettings(
        [FromBody] SystemSettingsUpdateRequest request,
        CancellationToken ct)
    {
        var settings = await systemSettingService.UpdateSystemSettingsAsync(request, ct);
        return Ok(settings);
    }
}
