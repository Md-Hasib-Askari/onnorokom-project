using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Profile;

public record ProfileDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool MustChangePassword);