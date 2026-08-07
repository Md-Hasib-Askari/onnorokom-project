using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role,
    AccountStatus Status);
