using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class AuthUser : BaseEntity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private init; }
    public AccountStatus Status { get; private set; } = AccountStatus.Pending;
    public bool IsActive { get; private set; } = true;
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }
    public string? PreviousRefreshToken { get; private set; }
    public DateTimeOffset? PreviousRefreshTokenGraceExpiresAt { get; private set; }
    public bool IsUsableAdmin => Role == UserRole.Admin && Status == AccountStatus.Approved && IsActive;
    public bool IsUsableTeacher => Role == UserRole.Teacher && Status == AccountStatus.Approved && IsActive;

    private AuthUser()
    {
    }

    public void Approve()
    {
        Status = AccountStatus.Approved;
        IsActive = true;
    }

    public void Reject()
    {
        Status = AccountStatus.Rejected;
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void ApplyStatus(AccountStatus status, bool isActive)
    {
        switch (status)
        {
            case AccountStatus.Approved:
                if (Status != AccountStatus.Approved)
                {
                    Approve();
                }

                if (isActive)
                {
                    Activate();
                }
                else
                {
                    Deactivate();
                }

                break;
            case AccountStatus.Rejected:
                Reject();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "Only Approved or Rejected statuses can be applied.");
        }
    }

    public void UpdateDetails(string fullName, string email)
    {
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public void SetRefreshToken(string refreshToken, DateTimeOffset expiresAt, DateTimeOffset graceExpiresAt)
    {
        if (RefreshToken is not null)
        {
            PreviousRefreshToken = RefreshToken;
            PreviousRefreshTokenGraceExpiresAt = graceExpiresAt;
        }

        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
        PreviousRefreshToken = null;
        PreviousRefreshTokenGraceExpiresAt = null;
    }

    /// <summary>
    /// True if <paramref name="token"/> is the immediately-prior refresh token and still within
    /// its grace window, i.e. a request that raced the rotation rather than one presenting a
    /// stale or stolen token.
    /// </summary>
    public bool IsPreviousRefreshToken(string token) =>
        PreviousRefreshToken == token
        && PreviousRefreshTokenGraceExpiresAt is { } graceExpiresAt
        && graceExpiresAt > DateTimeOffset.UtcNow;

    public static AuthUser CreatePending(string fullName, string email, string passwordHash, UserRole role)
    {
        return new AuthUser
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            Status = AccountStatus.Pending,
            IsActive = true
        };
    }

    public static AuthUser CreateApprovedAdmin(string fullName, string email, string passwordHash)
    {
        var admin = CreatePending(fullName, email, passwordHash, UserRole.Admin);
        admin.Approve();
        return admin;
    }
}
