using AssignmentSystem.Application.Common.Interfaces;
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
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var assignments = await teacherAssignmentService.GetMyAssignmentsAsync(ct);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await teacherAssignmentService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id}/submissions")]
    public async Task<IActionResult> GetSubmissions(Guid id, CancellationToken ct)
    {
        var submissions = await teacherSubmissionService.GetForAssignmentAsync(id, ct);
        return Ok(submissions);
    }
}