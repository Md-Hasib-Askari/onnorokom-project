using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.DTOs.Admin;

public record UserUpdateRequest(
    string FullName,
    string Email,
    AccountStatus Status,
    bool IsActive,
    Guid? StudentGradeId = null,
    TeacherProfileUpdateRequest? TeacherProfile = null,
    AdminProfileUpdateRequest? AdminProfile = null);
