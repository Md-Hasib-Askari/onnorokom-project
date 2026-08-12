using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAdminUserService
{
    Task<List<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<UserListItemDto> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default);
    Task<UserListItemDto> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default);
    Task DeleteUserAsync(Guid userId, CancellationToken ct = default);
    Task ResetPasswordAsync(Guid userId, CancellationToken ct = default);
}
