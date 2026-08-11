using AssignmentSystem.Application.Common.Validators;
using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Tests;

public class AssignmentCreateRequestValidatorTests
{
    private readonly AssignmentCreateRequestValidator _validator = new();

    private static AssignmentCreateRequest Request(
        DateTimeOffset? deadline = null,
        string title = "Essay",
        decimal maxMarks = 100)
        => new(title, null, Guid.NewGuid(), Guid.NewGuid(),
            deadline ?? DateTimeOffset.UtcNow.AddDays(7), maxMarks, false);

    [Fact]
    public void Validate_FutureDeadline_Passes()
    {
        var result = _validator.Validate(Request());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PastDeadline_Fails()
    {
        var result = _validator.Validate(Request(deadline: DateTimeOffset.UtcNow.AddDays(-1)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignmentCreateRequest.Deadline));
    }

    [Fact]
    public void Validate_EmptyTitle_Fails()
    {
        var result = _validator.Validate(Request(title: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignmentCreateRequest.Title));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_NonPositiveMaxMarks_Fails(decimal maxMarks)
    {
        var result = _validator.Validate(Request(maxMarks: maxMarks));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignmentCreateRequest.MaxMarks));
    }

    [Fact]
    public void Validate_MissingSection_Fails()
    {
        var request = new AssignmentCreateRequest("Essay", null, Guid.Empty, Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(7), 100, false);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignmentCreateRequest.SectionId));
    }
}