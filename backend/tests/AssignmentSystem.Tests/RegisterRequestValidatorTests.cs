using AssignmentSystem.Application.Common.Validators;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Theory]
    [InlineData("StrongPass1!")]
    [InlineData("P@ssw0rdX")]
    [InlineData("aB3!abcdefgh")]
    public void Validate_StrongPassword_Passes(string password)
    {
        var request = new RegisterRequest("Student One", "student@test.com", password, UserRole.Teacher);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChar1")]
    public void Validate_WeakPassword_Fails(string password)
    {
        var request = new RegisterRequest("Student One", "student@test.com", password, UserRole.Teacher);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    /// <summary>
    /// Sign-up never carries a section: the approving admin picks it, so a section-less student
    /// payload is well formed here. Whether the role is actually open is an admin setting checked
    /// in the service, not a validation rule.
    /// </summary>
    [Fact]
    public void Validate_StudentWithoutSection_Passes()
    {
        var request = new RegisterRequest("Student One", "student@test.com", "StrongPass1!", UserRole.Student);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AdminRole_Fails()
    {
        var request = new RegisterRequest("Admin One", "admin@test.com", "StrongPass1!", UserRole.Admin);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Role));
    }

    [Fact]
    public void Validate_UnknownRole_Fails()
    {
        var request = new RegisterRequest("Someone", "someone@test.com", "StrongPass1!", (UserRole)99);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Role));
    }
}