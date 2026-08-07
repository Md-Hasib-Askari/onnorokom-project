using AssignmentSystem.Application.DTOs.Subjects;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class AssignTeacherRequestValidator : AbstractValidator<AssignTeacherRequest>
{
    public AssignTeacherRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher is required.");
    }
}
