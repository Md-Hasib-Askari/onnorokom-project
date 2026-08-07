using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Auth;

public record RegisterRequest(string FullName, string Email, string Password, UserRole Role = UserRole.Student);
