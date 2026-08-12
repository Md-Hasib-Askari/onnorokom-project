using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Profile;

namespace AssignmentSystem.Application.Common.Interfaces;

/// <summary>
/// Self-service account operations for the logged-in user, distinct from
/// <see cref="IProfileRepository"/> which stores the role-specific (Teacher/Student/Admin) profile
/// entities.
/// </summary>
public interface IAccountService
{
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}