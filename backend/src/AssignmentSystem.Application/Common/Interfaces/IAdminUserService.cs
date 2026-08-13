using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Profile;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAdminUserService
{
    Task<PagedResult<UserListItemDto>> GetAllUsersAsync(
        PageRequest page,
        string? cursor,
        AccountStatus? status,
        UserRole? role,
        CancellationToken ct = default);
    Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserListItemDto> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default);
    Task<UserListItemDto> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default);
    Task DeleteUserAsync(Guid userId, CancellationToken ct = default);
    Task ResetPasswordAsync(Guid userId, CancellationToken ct = default);
}
