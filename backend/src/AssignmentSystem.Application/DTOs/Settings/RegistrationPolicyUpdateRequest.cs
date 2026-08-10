namespace AssignmentSystem.Application.DTOs.Settings;

public record RegistrationPolicyUpdateRequest(
    bool TeacherSelfRegistrationEnabled,
    bool StudentSelfRegistrationEnabled);
