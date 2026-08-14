using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/teacher/stats")]
[Authorize(Roles = nameof(UserRole.Teacher))]
public class TeacherStatsController(ITeacherStatsService teacherStatsService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var overview = await teacherStatsService.GetOverviewAsync(ct);
        return Ok(overview);
    }
}
