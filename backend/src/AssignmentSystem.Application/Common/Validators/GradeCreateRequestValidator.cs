using AssignmentSystem.Application.DTOs.Grades;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class GradeCreateRequestValidator : AbstractValidator<GradeCreateRequest>
{
    public GradeCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Grade name is required.")
            .MaximumLength(100).WithMessage("Grade name must not exceed 100 characters.");

        RuleFor(x => x.AcademicYear)
            .NotEmpty().WithMessage("Academic year is required.")
            .MaximumLength(20).WithMessage("Academic year must not exceed 20 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
