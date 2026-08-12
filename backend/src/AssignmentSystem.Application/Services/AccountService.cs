using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Profile;
using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Services;

public class AccountService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAccountService
{
    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        return ToDto(user);
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        user.UpdateDetails(request.FullName, user.Email);
        await userRepository.UpdateAsync(user, ct);
        return ToDto(user);
    }

    /// <summary>
    /// Reissues tokens after the change so the current session survives, while the prior refresh
    /// token (and therefore any other session) is revoked.
    /// </summary>
    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidCurrentPasswordException();
        }

        user.SetPassword(passwordHasher.Hash(request.NewPassword));

        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();
        user.SetRefreshToken(refreshToken, tokenService.RefreshTokenExpiresAt, tokenService.RefreshTokenGraceExpiresAt);
        await userRepository.UpdateAsync(user, ct);

        return new AuthResponse(
            accessToken,
            refreshToken,
            tokenService.AccessTokenExpiresAt,
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status,
            user.MustChangePassword);
    }

    private async Task<AuthUser> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");
    }

    private static ProfileDto ToDto(AuthUser user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.MustChangePassword);
}