namespace AssignmentSystem.Application.DTOs.Profile;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);