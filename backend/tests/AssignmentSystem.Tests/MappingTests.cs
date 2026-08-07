using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AutoMapper;

namespace AssignmentSystem.Tests;

public class MappingTests
{
    private readonly IMapper _mapper = TestMappers.CreateMapper();

    [Fact]
    public void Configuration_IsValid()
    {
        TestMappers.CreateMapper().ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void AuthUser_MapsToUserListItemDto()
    {
        var user = CreateUser();

        var dto = _mapper.Map<UserListItemDto>(user);

        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.FullName, dto.FullName);
        Assert.Equal(user.Email, dto.Email);
        Assert.Equal(user.Role, dto.Role);
        Assert.Equal(user.Status, dto.Status);
        Assert.Equal(user.CreatedAt, dto.CreatedAt);
    }

    private static AuthUser CreateUser()
    {
        var user = AuthUser.CreatePending("Jane Doe", "jane@test.com", "HASH:secret123", UserRole.Teacher);
        user.Approve();
        return user;
    }
}
