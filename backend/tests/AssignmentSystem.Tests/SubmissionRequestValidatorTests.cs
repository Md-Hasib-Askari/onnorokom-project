using AssignmentSystem.Application.Common.Validators;
using AssignmentSystem.Application.DTOs.Student;

namespace AssignmentSystem.Tests;

public class SubmissionRequestValidatorTests
{
    private readonly SubmissionCreateRequestValidator _createValidator = new();
    private readonly SubmissionUpdateRequestValidator _updateValidator = new();

    [Fact]
    public void Validate_ContentOnly_Passes()
    {
        Assert.True(_createValidator.Validate(new SubmissionCreateRequest("My answer", null)).IsValid);
    }

    [Fact]
    public void Validate_AttachmentOnly_Passes()
    {
        Assert.True(_createValidator.Validate(new SubmissionCreateRequest(null, "https://drive.example.com/a")).IsValid);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", null)]
    public void Validate_NeitherContentNorAttachment_Fails(string? content, string? attachmentUrl)
    {
        var result = _createValidator.Validate(new SubmissionCreateRequest(content, attachmentUrl));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter your work or attach a link.");
    }

    [Theory]
    [InlineData("drive.example.com/a")]
    [InlineData("ftp://files.example.com/a")]
    [InlineData("javascript:alert(1)")]
    public void Validate_AttachmentThatIsNotAnAbsoluteWebUrl_Fails(string attachmentUrl)
    {
        var result = _createValidator.Validate(new SubmissionCreateRequest(null, attachmentUrl));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubmissionCreateRequest.AttachmentUrl));
    }

    /// <summary>
    /// Editing runs the same rule set as submitting, so a student cannot wipe their work by
    /// sending an empty edit.
    /// </summary>
    [Fact]
    public void Validate_UpdateWithNeitherContentNorAttachment_Fails()
    {
        Assert.False(_updateValidator.Validate(new SubmissionUpdateRequest(null, null)).IsValid);
    }

    [Fact]
    public void Validate_UpdateWithContent_Passes()
    {
        Assert.True(_updateValidator.Validate(new SubmissionUpdateRequest("Reworked answer", null)).IsValid);
    }
}