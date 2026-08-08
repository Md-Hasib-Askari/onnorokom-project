using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IGradeRepository gradeRepository,
    IProfileRepository profileRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ITransactionService transactionService,
    IProfileProvisioningService profileProvisioningService) : IAuthService
{
    public async Task<AuthUser> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DuplicateEmailException(email);
        }

        await UserGuards.EnsureStudentGradeValidAsync(gradeRepository, request.Role, request.StudentGradeId, ct);

        var user = AuthUser.CreatePending(request.FullName, email, passwordHasher.Hash(request.Password), request.Role);

        await transactionService.ExecuteAsync(async transactionCt =>
        {
            await userRepository.AddAsync(user, transactionCt);
            await profileProvisioningService.CreateProfileAsync(user, request.StudentGradeId, transactionCt);
        }, ct);
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

        if (user is null)
        {
            throw new InvalidRefreshTokenException();
        }

        if (user.IsPreviousRefreshToken(refreshToken))
        {
            EnsureUsable(user);
            return BuildResponse(user, tokenService.CreateAccessToken(user), user.RefreshToken!);
        }

        if (user.RefreshToken != refreshToken
            || user.RefreshTokenExpiresAt is null
            || user.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        EnsureUsable(user);
        return await IssueTokensAsync(user, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await userRepository.GetByRefreshTokenAsync(refreshToken, ct);
        if (user is null)
        {
            return;
        }

        user.RevokeRefreshToken();
        await userRepository.UpdateAsync(user, ct);
    }

    public async Task<AuthUser> ApproveAsync(Guid userId, bool approve, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        if (approve)
        {
            if (user.Status == AccountStatus.Approved)
            {
                throw new DomainException("User is already approved.");
            }

            user.Approve();
        }
        else
        {
            await UserGuards.EnsureNotLastUsableAdminAsync(
                userRepository,
                user.IsUsableAdmin,
                "The last admin account cannot be rejected.",
                ct);

            user.Reject();
        }

        await userRepository.UpdateAsync(user, ct);
        return user;
    }

    public async Task<List<UserListItemDto>> GetPendingUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetByStatusAsync(AccountStatus.Pending, ct);
        return await UserListItemDtoFactory.BuildAsync(users, profileRepository, gradeRepository, ct);
    }

    private void EnsureUsable(AuthUser user)
    {
        switch (user.Status)
        {
            case AccountStatus.Pending:
                throw new AccountPendingException();
            case AccountStatus.Rejected:
                throw new AccountRejectedException();
        }

        if (!user.IsActive)
        {
            throw new AccountInactiveException();
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(AuthUser user, CancellationToken ct)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();
        user.SetRefreshToken(refreshToken, tokenService.RefreshTokenExpiresAt, tokenService.RefreshTokenGraceExpiresAt);
        await userRepository.UpdateAsync(user, ct);

        return BuildResponse(user, accessToken, refreshToken);
    }

    private AuthResponse BuildResponse(AuthUser user, string accessToken, string refreshToken) =>
        new(
            accessToken,
            refreshToken,
            tokenService.AccessTokenExpiresAt,
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status);
}
