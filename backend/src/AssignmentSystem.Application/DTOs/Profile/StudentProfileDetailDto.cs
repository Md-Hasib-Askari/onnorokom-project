using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Profile;

/// <summary>One student profile block as returned by the profile and admin user-detail reads.</summary>
public record StudentProfileDetailDto(
    Guid SectionId,
    string? SectionName,
    string? GradeName,
    string? RollNumber,
    DateTimeOffset? DateOfBirth,
    Gender? Gender,
    string? GuardianName,
    string? GuardianPhone,
    string? Address,
    DateTimeOffset? AdmissionDate);
