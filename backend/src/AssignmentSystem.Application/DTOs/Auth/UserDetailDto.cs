using AssignmentSystem.Application.DTOs.Profile;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Auth;

/// <summary>
/// The admin's full read of one user, including role-specific profile blocks. This is the edit
/// dialog's source of truth; the list DTO only carries what the table columns need.
/// </summary>
public record UserDetailDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    AccountStatus Status,
    DateTimeOffset CreatedAt,
    bool IsActive,
    StudentProfileDetailDto? StudentProfile,
    TeacherProfileDetailDto? TeacherProfile,
    AdminProfileDetailDto? AdminProfile);
