using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Subjects;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/subjects")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminSubjectController(
    ISubjectService subjectService,
    IValidator<SubjectCreateRequest> createValidator,
    IValidator<SubjectUpdateRequest> updateValidator,
    IValidator<AssignTeacherRequest> assignTeacherValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var subjects = await subjectService.GetAllAsync(ct);
        return Ok(subjects);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubjectCreateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var subject = await subjectService.CreateAsync(request, ct);
        return Ok(subject);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SubjectUpdateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var subject = await subjectService.UpdateAsync(id, request, ct);
        return Ok(subject);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await subjectService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id}/teacher")]
    public async Task<IActionResult> AssignTeacher(Guid id, [FromBody] AssignTeacherRequest request, CancellationToken ct)
    {
        var validation = await assignTeacherValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var subject = await subjectService.AssignTeacherAsync(id, request.TeacherId, ct);
        return Ok(subject);
    }

    [HttpDelete("{id}/teacher")]
    public async Task<IActionResult> UnassignTeacher(Guid id, CancellationToken ct)
    {
        var subject = await subjectService.UnassignTeacherAsync(id, ct);
        return Ok(subject);
    }
}
