using AssignmentSystem.Application.DTOs.Admin;

namespace AssignmentSystem.Application.DTOs.Profile;

public record UpdateProfileRequest(
    string FullName,
    StudentProfileUpdateRequest? StudentProfile = null,
    TeacherProfileUpdateRequest? TeacherProfile = null,
    AdminProfileUpdateRequest? AdminProfile = null);