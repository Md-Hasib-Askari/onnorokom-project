using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Enums;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain at least one special character.");

        // Whether Teacher and Student are actually open is an admin-controlled setting enforced in
        // AuthService; only the Admin role is barred unconditionally, so it stays in the validator.
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.")
            .NotEqual(UserRole.Admin).WithMessage("Public registration is not allowed for the Admin role.");
    }
}
