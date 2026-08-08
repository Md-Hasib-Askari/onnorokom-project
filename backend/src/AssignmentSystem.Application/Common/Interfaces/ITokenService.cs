using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(AuthUser user);
    DateTimeOffset AccessTokenExpiresAt { get; }
    string CreateRefreshToken();
    DateTimeOffset RefreshTokenExpiresAt { get; }

    /// <summary>
    /// How long a just-rotated refresh token keeps resolving after being replaced, so a request
    /// racing a concurrent refresh (same token, both in flight) gets the new token pair back
    /// instead of being treated as invalid.
    /// </summary>
    DateTimeOffset RefreshTokenGraceExpiresAt { get; }
}
