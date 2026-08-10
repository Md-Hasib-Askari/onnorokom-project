using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthUser> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task<AuthUser> ApproveAsync(Guid userId, bool approve, Guid? studentSectionId = null,
        CancellationToken ct = default);
    Task<List<UserListItemDto>> GetPendingUsersAsync(CancellationToken ct = default);
}
