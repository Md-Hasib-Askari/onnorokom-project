using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/teacher/assignments")]
[Authorize(Roles = nameof(UserRole.Teacher))]
public class TeacherAssignmentController(
    ITeacherAssignmentService teacherAssignmentService,
    ITeacherSubmissionService teacherSubmissionService,
    IValidator<AssignmentCreateRequest> createValidator,
    IValidator<AssignmentUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        CancellationToken ct)
    {
        var assignments = await teacherAssignmentService.GetMyAssignmentsAsync(new PageRequest(limit), cursor, ct);
        return Ok(assignments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var assignment = await teacherAssignmentService.GetByIdAsync(id, ct);
        return Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssignmentCreateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var assignment = await teacherAssignmentService.CreateAsync(request, ct);
        return Ok(assignment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AssignmentUpdateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var assignment = await teacherAssignmentService.UpdateAsync(id, request, ct);
        return Ok(assignment);
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var assignment = await teacherAssignmentService.PublishAsync(id, ct);
        return Ok(assignment);
    }

    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
    {
        var assignment = await teacherAssignmentService.UnpublishAsync(id, ct);
        return Ok(assignment);
    }

    [HttpPost("{id}/close-submissions")]
    public async Task<IActionResult> CloseSubmissions(Guid id, CancellationToken ct)
    {
        var assignment = await teacherAssignmentService.CloseSubmissionsAsync(id, ct);
        return Ok(assignment);
    }

    [HttpPost("{id}/reopen-submissions")]
    public async Task<IActionResult> ReopenSubmissions(Guid id, CancellationToken ct)
    {
        var assignment = await teacherAssignmentService.ReopenSubmissionsAsync(id, ct);
        return Ok(assignment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await teacherAssignmentService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id}/submissions")]
    public async Task<IActionResult> GetSubmissions(
        Guid id,
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        CancellationToken ct)
    {
        var submissions = await teacherSubmissionService.GetForAssignmentAsync(id, new PageRequest(limit), cursor, ct);
        return Ok(submissions);
    }
}