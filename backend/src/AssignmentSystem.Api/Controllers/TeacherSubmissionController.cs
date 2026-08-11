using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Teacher;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/teacher/submissions")]
[Authorize(Roles = nameof(UserRole.Teacher))]
public class TeacherSubmissionController(
    ITeacherSubmissionService teacherSubmissionService,
    IValidator<GradeSubmissionRequest> gradeValidator) : ControllerBase
{
    [HttpPut("{id}/grade")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionRequest request, CancellationToken ct)
    {
        var validation = await gradeValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var submission = await teacherSubmissionService.GradeAsync(id, request, ct);
        return Ok(submission);
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> Return(Guid id, CancellationToken ct)
    {
        var submission = await teacherSubmissionService.ReturnAsync(id, ct);
        return Ok(submission);
    }
}