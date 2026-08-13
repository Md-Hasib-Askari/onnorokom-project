using AssignmentSystem.Application.DTOs.Profile;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.StudentProfile!)
            .SetValidator(new StudentProfileUpdateRequestValidator())
            .When(x => x.StudentProfile is not null);

        RuleFor(x => x.TeacherProfile!)
            .SetValidator(new TeacherProfileUpdateRequestValidator())
            .When(x => x.TeacherProfile is not null);

        RuleFor(x => x.AdminProfile!)
            .SetValidator(new AdminProfileUpdateRequestValidator())
            .When(x => x.AdminProfile is not null);
    }
}