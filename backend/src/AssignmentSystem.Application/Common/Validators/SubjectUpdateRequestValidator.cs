using AssignmentSystem.Application.DTOs.Subjects;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class SubjectUpdateRequestValidator : AbstractValidator<SubjectUpdateRequest>
{
    public SubjectUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(100).WithMessage("Subject name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Subject code is required.")
            .MaximumLength(20).WithMessage("Subject code must not exceed 20 characters.");

        RuleFor(x => x.GradeId)
            .NotEmpty().WithMessage("Grade is required.");
    }
}
