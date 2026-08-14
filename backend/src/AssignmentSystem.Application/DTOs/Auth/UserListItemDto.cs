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
    Guid? StudentSectionId = null,
    string? SectionName = null,
    string? GradeName = null,
    string? TeacherCode = null,
    string? RollNumber = null,
    DateTimeOffset? DateOfBirth = null,
    Gender? Gender = null,
    string? GuardianName = null,
    string? GuardianPhone = null,
    string? Address = null,
    DateTimeOffset? AdmissionDate = null);
