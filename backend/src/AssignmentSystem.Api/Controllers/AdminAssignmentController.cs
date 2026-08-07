using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminAssignmentController(IAdminQueryService adminQueryService) : ControllerBase
{
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAllAssignments(CancellationToken ct)
    {
        var assignments = await adminQueryService.GetAllAssignmentsAsync(ct);
        return Ok(assignments);
    }

    [HttpGet("submissions")]
    public async Task<IActionResult> GetAllSubmissions(CancellationToken ct)
    {
        var submissions = await adminQueryService.GetAllSubmissionsAsync(ct);
        return Ok(submissions);
    }
}
