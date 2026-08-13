using AssignmentSystem.Application.DTOs.Settings;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISystemSettingService
{
    Task<SystemSettingsDto> GetSystemSettingsAsync(CancellationToken ct = default);

    Task<SystemSettingsDto> UpdateSystemSettingsAsync(
        SystemSettingsUpdateRequest request,
        CancellationToken ct = default);

    Task<RegistrationPolicyDto> GetRegistrationPolicyAsync(CancellationToken ct = default);

    /// <summary>
    /// Throws when the current policy does not accept self-registration for <paramref name="role"/>.
    /// </summary>
    Task EnsureSelfRegistrationAllowedAsync(UserRole role, CancellationToken ct = default);

    Task<ProfileEditPolicyDto> GetProfileEditPolicyAsync(CancellationToken ct = default);

    /// <summary>
    /// Throws when the current policy does not allow self-service profile edits for <paramref name="role"/>.
    /// Admins are always allowed and never throw.
    /// </summary>
    Task EnsureProfileEditAllowedAsync(UserRole role, CancellationToken ct = default);
}
