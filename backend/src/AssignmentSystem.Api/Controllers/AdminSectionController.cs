using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/sections")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminSectionController(
    ISectionService sectionService,
    ISectionSubjectService sectionSubjectService,
    IValidator<SectionCreateRequest> createValidator,
    IValidator<SectionUpdateRequest> updateValidator,
    IValidator<AssignSectionSubjectTeacherRequest> assignTeacherValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var sections = await sectionService.GetAllAsync(ct);
        return Ok(sections);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SectionCreateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var section = await sectionService.CreateAsync(request, ct);
        return Ok(section);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SectionUpdateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var section = await sectionService.UpdateAsync(id, request, ct);
        return Ok(section);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sectionService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{sectionId}/subjects")]
    public async Task<IActionResult> GetSectionSubjects(Guid sectionId, CancellationToken ct)
    {
        var subjects = await sectionSubjectService.GetSectionSubjectsAsync(sectionId, ct);
        return Ok(subjects);
    }

    [HttpPost("{sectionId}/subjects/{subjectId}/teacher")]
    public async Task<IActionResult> AssignSubjectTeacher(
        Guid sectionId,
        Guid subjectId,
        [FromBody] AssignSectionSubjectTeacherRequest request,
        CancellationToken ct)
    {
        var validation = await assignTeacherValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var result = await sectionSubjectService.AssignTeacherAsync(sectionId, subjectId, request.TeacherId, ct);
        return Ok(result);
    }

    [HttpDelete("{sectionId}/subjects/{subjectId}/teacher")]
    public async Task<IActionResult> UnassignSubjectTeacher(Guid sectionId, Guid subjectId, CancellationToken ct)
    {
        var result = await sectionSubjectService.UnassignTeacherAsync(sectionId, subjectId, ct);
        return Ok(result);
    }
}
