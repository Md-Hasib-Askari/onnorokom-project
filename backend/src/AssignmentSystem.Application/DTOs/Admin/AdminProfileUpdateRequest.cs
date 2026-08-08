namespace AssignmentSystem.Application.DTOs.Admin;

public record AdminProfileUpdateRequest(
    string? Position,
    string? PhoneNumber);