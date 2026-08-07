using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/admin/grades")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminGradeController(
    IGradeService gradeService,
    IValidator<GradeCreateRequest> createValidator,
    IValidator<GradeUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var grades = await gradeService.GetAllAsync(ct);
        return Ok(grades);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GradeCreateRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var grade = await gradeService.CreateAsync(request, ct);
        return Ok(grade);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] GradeUpdateRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var grade = await gradeService.UpdateAsync(id, request, ct);
        return Ok(grade);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await gradeService.DeleteAsync(id, ct);
        return NoContent();
    }
}
