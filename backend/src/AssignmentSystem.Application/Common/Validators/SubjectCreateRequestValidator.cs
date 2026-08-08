using AssignmentSystem.Application.DTOs.Subjects;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class SubjectCreateRequestValidator : AbstractValidator<SubjectCreateRequest>
{
    public SubjectCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(100).WithMessage("Subject name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Subject code must not exceed 20 characters.");

        RuleFor(x => x.GradeId)
            .NotEmpty().WithMessage("Grade is required.");
    }
}
