using AssignmentSystem.Application.DTOs.Teacher;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

/// <summary>
/// The upper bound is not here: it is the parent assignment's <c>MaxMarks</c>, which the request
/// does not carry, so <c>TeacherSubmissionService</c> enforces it.
/// </summary>
public class GradeSubmissionRequestValidator : AbstractValidator<GradeSubmissionRequest>
{
    public GradeSubmissionRequestValidator()
    {
        RuleFor(x => x.Marks)
            .GreaterThanOrEqualTo(0).WithMessage("Marks cannot be negative.");

        RuleFor(x => x.Feedback)
            .MaximumLength(2000).WithMessage("Feedback must not exceed 2000 characters.");
    }
}