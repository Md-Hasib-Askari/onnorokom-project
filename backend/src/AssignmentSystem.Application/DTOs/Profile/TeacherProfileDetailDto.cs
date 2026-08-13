namespace AssignmentSystem.Application.DTOs.Profile;

/// <summary>One teacher profile block as returned by the profile and admin user-detail reads.</summary>
public record TeacherProfileDetailDto(
    string? TeacherCode,
    string? Department,
    string? Designation,
    string? Qualification,
    string? PhoneNumber,
    string? Address,
    DateTimeOffset? DateOfJoining);
