using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Auth;

/// <summary>
/// Self-registration carries no section: a student picks none, and the admin assigns one when
/// approving the account. Admin-created users go through the admin user endpoints instead.
/// </summary>
public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role = UserRole.Student);
