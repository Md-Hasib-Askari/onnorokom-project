using AssignmentSystem.Application.DTOs.Admin;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class StudentProfileUpdateRequestValidator : AbstractValidator<StudentProfileUpdateRequest>
{
    public StudentProfileUpdateRequestValidator()
    {
        RuleFor(x => x.RollNumber).MaximumLength(30);
        RuleFor(x => x.GuardianName).MaximumLength(100);
        RuleFor(x => x.GuardianPhone).MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}
