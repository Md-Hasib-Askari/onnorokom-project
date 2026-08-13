namespace AssignmentSystem.Application.DTOs.Profile;

/// <summary>One admin profile block as returned by the profile and admin user-detail reads.</summary>
public record AdminProfileDetailDto(
    string? Position,
    string? PhoneNumber);
