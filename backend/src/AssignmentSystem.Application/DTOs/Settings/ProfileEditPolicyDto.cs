namespace AssignmentSystem.Application.DTOs.Settings;

/// <summary>Whether each non-admin role may edit its own profile, for the profile page.</summary>
public record ProfileEditPolicyDto(
    bool TeacherProfileSelfEditEnabled,
    bool StudentProfileSelfEditEnabled);
