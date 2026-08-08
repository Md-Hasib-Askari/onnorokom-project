namespace AssignmentSystem.Application.DTOs.Admin;

public record TeacherProfileUpdateRequest(
    string? Department,
    string? Designation,
    string? Qualification,
    string? PhoneNumber,
    string? Address,
    DateTimeOffset? DateOfJoining);