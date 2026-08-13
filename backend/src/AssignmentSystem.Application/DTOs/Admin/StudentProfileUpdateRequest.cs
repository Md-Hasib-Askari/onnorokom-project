using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Admin;

public record StudentProfileUpdateRequest(
    string? RollNumber,
    DateTimeOffset? DateOfBirth,
    Gender? Gender,
    string? GuardianName,
    string? GuardianPhone,
    string? Address,
    DateTimeOffset? AdmissionDate);
