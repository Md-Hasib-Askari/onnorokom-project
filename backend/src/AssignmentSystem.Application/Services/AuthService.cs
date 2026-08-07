using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService) : IAuthService
{
    public async Task<AuthUser> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DuplicateEmailException(email);
        }

        var user = new AuthUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role,
            Status = AccountStatus.Pending,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.AddAsync(user, ct);
        return user;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        EnsureUsable(user);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await userRepository.GetByRefreshTokenAsync(refreshToken, ct);

        if (user is null
            || user.RefreshTokenExpiresAt is null
            || user.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        EnsureUsable(user);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthUser> ApproveAsync(Guid userId, bool approve, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        user.Status = approve ? AccountStatus.Approved : AccountStatus.Rejected;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await userRepository.UpdateAsync(user, ct);
        return user;
    }

    public async Task<List<UserListItemDto>> GetPendingUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetByStatusAsync(AccountStatus.Pending, ct);
        return users
            .Select(u => new UserListItemDto(u.Id, u.FullName, u.Email, u.Role, u.Status, u.CreatedAt))
            .ToList();
    }

    private void EnsureUsable(AuthUser user)
    {
        if (!user.IsActive)
        {
            throw new AccountInactiveException();
        }

        switch (user.Status)
        {
            case AccountStatus.Pending:
                throw new AccountPendingException();
            case AccountStatus.Rejected:
                throw new AccountRejectedException();
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(AuthUser user, CancellationToken ct)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        user.RefreshToken = tokenService.CreateRefreshToken();
        user.RefreshTokenExpiresAt = tokenService.RefreshTokenExpiresAt;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await userRepository.UpdateAsync(user, ct);

        return new AuthResponse(
            accessToken,
            user.RefreshToken,
            tokenService.AccessTokenExpiresAt,
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status);
    }
}
