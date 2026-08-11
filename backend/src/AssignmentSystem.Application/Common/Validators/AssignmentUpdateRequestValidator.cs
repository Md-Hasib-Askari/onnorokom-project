using AssignmentSystem.Application.DTOs.Teacher;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

/// <summary>
/// Deliberately has no future-deadline rule. A teacher must be able to correct a deadline that
/// has already passed, and shortening one to close an assignment early is a legitimate action.
/// </summary>
public class AssignmentUpdateRequestValidator : AbstractValidator<AssignmentUpdateRequest>
{
    public AssignmentUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Assignment title is required.")
            .MaximumLength(200).WithMessage("Assignment title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0).WithMessage("Maximum marks must be greater than zero.");
    }
}