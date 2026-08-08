using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthUser> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<AuthUser> ApproveAsync(Guid userId, bool approve, CancellationToken ct = default);
    Task<List<UserListItemDto>> GetPendingUsersAsync(CancellationToken ct = default);
}
