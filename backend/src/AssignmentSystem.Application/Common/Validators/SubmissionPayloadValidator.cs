using AssignmentSystem.Application.DTOs.Student;
using FluentValidation;

namespace AssignmentSystem.Application.Common.Validators;

/// <summary>
/// Shared rules for submitting and editing. Written once against
/// <see cref="ISubmissionPayload"/> so the two request types cannot accept different work.
/// </summary>
public abstract class SubmissionPayloadValidator<T> : AbstractValidator<T> where T : ISubmissionPayload
{
    protected SubmissionPayloadValidator()
    {
        // Either field alone is a complete answer, so the rule is on the pair rather than on
        // each field: typed work with no link is fine, and so is a link with no typed work.
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.AttachmentUrl))
            .WithName(nameof(ISubmissionPayload.Content))
            .WithMessage("Enter your work or attach a link.");

        RuleFor(x => x.Content)
            .MaximumLength(10000).WithMessage("Your work must not exceed 10000 characters.");

        RuleFor(x => x.AttachmentUrl)
            .MaximumLength(2000).WithMessage("Attachment link must not exceed 2000 characters.")
            .Must(BeAnAbsoluteWebUrl).WithMessage("Attachment link must be a valid http or https URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.AttachmentUrl));
    }

    /// <summary>
    /// Relative or exotic-scheme values are rejected outright: the UI renders this straight into an
    /// anchor, so anything that is not plain http(s) has no safe rendering.
    /// </summary>
    private static bool BeAnAbsoluteWebUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}