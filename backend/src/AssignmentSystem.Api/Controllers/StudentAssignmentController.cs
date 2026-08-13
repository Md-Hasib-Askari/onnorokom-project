using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Student;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

/// <summary>
/// The submission is keyed by its assignment rather than by its own id, which keeps submission ids
/// out of the student-facing API and matches the one-row-per-student-per-assignment rule.
/// </summary>
[ApiController]
[Route("api/student/assignments")]
[Authorize(Roles = nameof(UserRole.Student))]
public class StudentAssignmentController(
    IStudentAssignmentService studentAssignmentService,
    IValidator<SubmissionCreateRequest> createValidator,
    IValidator<SubmissionUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        CancellationToken ct)
    {
        var assignments = await studentAssignmentService.GetMyAssignmentsAsync(new PageRequest(limit), cursor, ct);
        return Ok(assignments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var assignment = await studentAssignmentService.GetByIdAsync(id, ct);
        return Ok(assignment);
    }

    [HttpPost("{id}/submission")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmissionCreateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var assignment = await studentAssignmentService.SubmitAsync(id, request, ct);
        return Ok(assignment);
    }

    [HttpPut("{id}/submission")]
    public async Task<IActionResult> UpdateSubmission(Guid id, [FromBody] SubmissionUpdateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var assignment = await studentAssignmentService.UpdateSubmissionAsync(id, request, ct);
        return Ok(assignment);
    }
}