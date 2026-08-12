using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
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
    public async Task<IActionResult> GetAllAssignments(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        CancellationToken ct)
    {
        var assignments = await adminQueryService.GetAllAssignmentsAsync(new PageRequest(limit), cursor, ct);
        return Ok(assignments);
    }

    [HttpGet("submissions")]
    public async Task<IActionResult> GetAllSubmissions(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        CancellationToken ct)
    {
        var submissions = await adminQueryService.GetAllSubmissionsAsync(new PageRequest(limit), cursor, ct);
        return Ok(submissions);
    }
}
