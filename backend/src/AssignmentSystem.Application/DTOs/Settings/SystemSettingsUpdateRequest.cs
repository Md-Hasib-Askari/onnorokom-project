namespace AssignmentSystem.Application.DTOs.Settings;

/// <summary>
/// All four flags are always sent, so a save writes the admin's full intent rather than a delta.
/// </summary>
public record SystemSettingsUpdateRequest(
    bool TeacherSelfRegistrationEnabled,
    bool StudentSelfRegistrationEnabled,
    bool TeacherProfileSelfEditEnabled,
    bool StudentProfileSelfEditEnabled);
