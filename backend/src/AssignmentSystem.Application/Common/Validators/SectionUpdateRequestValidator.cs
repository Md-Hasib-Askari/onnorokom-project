using AssignmentSystem.Application.DTOs.Sections;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class SectionUpdateRequestValidator : AbstractValidator<SectionUpdateRequest>
{
    public SectionUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Section name is required.")
            .MaximumLength(100).WithMessage("Section name must not exceed 100 characters.");

        RuleFor(x => x.GradeId)
            .NotEmpty().WithMessage("Grade is required.");
    }
}
