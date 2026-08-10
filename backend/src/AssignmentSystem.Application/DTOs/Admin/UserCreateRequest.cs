using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Admin;

public record UserCreateRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    Guid? StudentSectionId = null);
