using AssignmentSystem.Application.DTOs.Admin;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class AdminProfileUpdateRequestValidator : AbstractValidator<AdminProfileUpdateRequest>
{
    public AdminProfileUpdateRequestValidator()
    {
        RuleFor(x => x.Position).MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
    }
}