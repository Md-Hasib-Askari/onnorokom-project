using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Domain.Enums;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status.")
            .NotEqual(AccountStatus.Pending).WithMessage("Pending cannot be set via user update; use the approval endpoint.");

        RuleFor(x => x.TeacherProfile!)
            .SetValidator(new TeacherProfileUpdateRequestValidator())
            .When(x => x.TeacherProfile is not null);

        RuleFor(x => x.AdminProfile!)
            .SetValidator(new AdminProfileUpdateRequestValidator())
            .When(x => x.AdminProfile is not null);
    }
}
