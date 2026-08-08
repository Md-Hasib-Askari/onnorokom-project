using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Auth;

public record UserListItemDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    AccountStatus Status,
    DateTimeOffset CreatedAt,
    bool IsActive,
    Guid? StudentGradeId = null,
    string? GradeName = null);
