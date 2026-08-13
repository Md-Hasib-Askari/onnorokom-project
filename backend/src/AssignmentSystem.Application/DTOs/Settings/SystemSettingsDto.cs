namespace AssignmentSystem.Application.DTOs.Settings;

/// <summary>Every admin-tunable system setting in one payload, fetched and saved atomically.</summary>
public record SystemSettingsDto(
    bool TeacherSelfRegistrationEnabled,
    bool StudentSelfRegistrationEnabled,
    bool TeacherProfileSelfEditEnabled,
    bool StudentProfileSelfEditEnabled);
