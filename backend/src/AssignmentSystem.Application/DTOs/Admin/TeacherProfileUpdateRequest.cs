namespace AssignmentSystem.Application.DTOs.Admin;

public record TeacherProfileUpdateRequest(
    string? TeacherCode,
    string? Department,
    string? Designation,
    string? Qualification,
    string? PhoneNumber,
    string? Address,
    DateTimeOffset? DateOfJoining);