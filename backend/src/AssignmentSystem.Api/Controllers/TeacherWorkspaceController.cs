using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

/// <summary>
/// What the signed-in teacher has been given to work with. Separate from the assignment routes
/// because these answer "what may I create against", not "what have I created".
/// </summary>
[ApiController]
[Route("api/teacher")]
[Authorize(Roles = nameof(UserRole.Teacher))]
public class TeacherWorkspaceController(ITeacherAssignmentService teacherAssignmentService) : ControllerBase
{
    [HttpGet("section-subjects")]
    public async Task<IActionResult> GetMySectionSubjects(CancellationToken ct)
    {
        var sectionSubjects = await teacherAssignmentService.GetMySectionSubjectsAsync(ct);
        return Ok(sectionSubjects);
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetMyStudents(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        CancellationToken ct)
    {
        var students = await teacherAssignmentService.GetMyStudentsAsync(new PageRequest(limit), cursor, ct);
        return Ok(students);
    }
}