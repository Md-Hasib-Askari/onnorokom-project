using AssignmentSystem.Application.Common.Validators;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class UserUpdateRequestValidatorTests
{
    private readonly UserUpdateRequestValidator _validator = new();

    [Fact]
    public void Validate_PendingStatus_Fails()
    {
        var request = new UserUpdateRequest("Name", "a@test.com", AccountStatus.Pending, true);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UserUpdateRequest.Status));
    }

    [Theory]
    [InlineData(AccountStatus.Approved)]
    [InlineData(AccountStatus.Rejected)]
    public void Validate_NonPendingStatus_Passes(AccountStatus status)
    {
        var request = new UserUpdateRequest("Name", "a@test.com", status, true);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
