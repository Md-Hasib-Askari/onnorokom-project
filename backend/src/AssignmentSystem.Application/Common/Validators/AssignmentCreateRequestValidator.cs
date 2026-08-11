using AssignmentSystem.Application.DTOs.Teacher;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class AssignmentCreateRequestValidator : AbstractValidator<AssignmentCreateRequest>
{
    public AssignmentCreateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Assignment title is required.")
            .MaximumLength(200).WithMessage("Assignment title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("Section is required.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject is required.");

        // Evaluated per validation rather than captured once, so a long-lived validator instance
        // cannot go on comparing against the time it was constructed.
        RuleFor(x => x.Deadline)
            .Must(deadline => deadline > DateTimeOffset.UtcNow)
            .WithMessage("Deadline must be in the future.");

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0).WithMessage("Maximum marks must be greater than zero.");
    }
}