using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminStatsController(IAdminStatsService adminStatsService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var overview = await adminStatsService.GetOverviewAsync(ct);
        return Ok(overview);
    }
}
