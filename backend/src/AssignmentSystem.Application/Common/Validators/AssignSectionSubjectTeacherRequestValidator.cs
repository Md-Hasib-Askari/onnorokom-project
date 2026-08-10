using AssignmentSystem.Application.DTOs.Sections;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class AssignSectionSubjectTeacherRequestValidator : AbstractValidator<AssignSectionSubjectTeacherRequest>
{
    public AssignSectionSubjectTeacherRequestValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher is required.");
    }
}
