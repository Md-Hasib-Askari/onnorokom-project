using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Domain.Enums;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator()
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

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.");

        RuleFor(x => x.StudentSectionId)
            .NotNull().WithMessage("A section is required for student users.")
            .When(x => x.Role == UserRole.Student);
    }
}
