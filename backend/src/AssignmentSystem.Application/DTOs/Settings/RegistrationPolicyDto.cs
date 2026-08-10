namespace AssignmentSystem.Application.DTOs.Settings;

/// <summary>
/// Which roles the public registration endpoint currently accepts. Safe to serve anonymously:
/// it carries policy flags only, never the settings table itself.
/// </summary>
public record RegistrationPolicyDto(
    bool TeacherSelfRegistrationEnabled,
    bool StudentSelfRegistrationEnabled);
