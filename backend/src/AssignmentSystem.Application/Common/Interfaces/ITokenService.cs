using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(AuthUser user);
    DateTimeOffset AccessTokenExpiresAt { get; }
    string CreateRefreshToken();
    DateTimeOffset RefreshTokenExpiresAt { get; }
}
