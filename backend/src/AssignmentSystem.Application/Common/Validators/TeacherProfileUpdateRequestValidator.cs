using AssignmentSystem.Application.DTOs.Admin;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class TeacherProfileUpdateRequestValidator : AbstractValidator<TeacherProfileUpdateRequest>
{
    public TeacherProfileUpdateRequestValidator()
    {
        RuleFor(x => x.TeacherCode).MaximumLength(50);
        RuleFor(x => x.Department).MaximumLength(100);
        RuleFor(x => x.Designation).MaximumLength(100);
        RuleFor(x => x.Qualification).MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}