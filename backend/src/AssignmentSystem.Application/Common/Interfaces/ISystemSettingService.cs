using AssignmentSystem.Application.DTOs.Settings;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISystemSettingService
{
    Task<RegistrationPolicyDto> GetRegistrationPolicyAsync(CancellationToken ct = default);

    Task<RegistrationPolicyDto> UpdateRegistrationPolicyAsync(
        RegistrationPolicyUpdateRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Throws when the current policy does not accept self-registration for <paramref name="role"/>.
    /// </summary>
    Task EnsureSelfRegistrationAllowedAsync(UserRole role, CancellationToken ct = default);
}
