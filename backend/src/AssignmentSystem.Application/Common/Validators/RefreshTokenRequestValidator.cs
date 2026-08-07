using AssignmentSystem.Application.DTOs.Auth;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
