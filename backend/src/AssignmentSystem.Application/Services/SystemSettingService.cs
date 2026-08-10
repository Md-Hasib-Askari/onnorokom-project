using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Settings;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class SystemSettingService(ISystemSettingRepository systemSettingRepository) : ISystemSettingService
{
    /// <summary>
    /// Applied when a key has no row at all. Deliberately restrictive: a database that missed the
    /// seed should close registration rather than open it to a role the admin never enabled.
    /// </summary>
    private const bool MissingSettingFallback = false;

    public async Task<RegistrationPolicyDto> GetRegistrationPolicyAsync(CancellationToken ct = default)
    {
        var byKey = await LoadByKeyAsync(ct);

        return new RegistrationPolicyDto(
            ReadBoolean(byKey, SystemSettingKey.TeacherSelfRegistrationEnabled),
            ReadBoolean(byKey, SystemSettingKey.StudentSelfRegistrationEnabled));
    }

    public async Task<RegistrationPolicyDto> UpdateRegistrationPolicyAsync(
        RegistrationPolicyUpdateRequest request,
        CancellationToken ct = default)
    {
        var byKey = await LoadByKeyAsync(ct);

        var updated = new List<SystemSetting>
        {
            Apply(byKey, SystemSettingKey.TeacherSelfRegistrationEnabled, request.TeacherSelfRegistrationEnabled),
            Apply(byKey, SystemSettingKey.StudentSelfRegistrationEnabled, request.StudentSelfRegistrationEnabled)
        };

        await systemSettingRepository.UpsertAsync(updated, ct);

        return new RegistrationPolicyDto(
            request.TeacherSelfRegistrationEnabled,
            request.StudentSelfRegistrationEnabled);
    }

    public async Task EnsureSelfRegistrationAllowedAsync(UserRole role, CancellationToken ct = default)
    {
        // Admin is never self-registerable, and there is no setting that can turn it on. Guarding
        // here as well as in the validator keeps the rule true for any future caller of the service.
        if (role == UserRole.Admin)
        {
            throw new RegistrationDisabledException(role);
        }

        var policy = await GetRegistrationPolicyAsync(ct);

        var allowed = role switch
        {
            UserRole.Teacher => policy.TeacherSelfRegistrationEnabled,
            UserRole.Student => policy.StudentSelfRegistrationEnabled,
            _ => false
        };

        if (!allowed)
        {
            throw new RegistrationDisabledException(role);
        }
    }

    private async Task<Dictionary<SystemSettingKey, SystemSetting>> LoadByKeyAsync(CancellationToken ct)
    {
        var settings = await systemSettingRepository.GetAllAsync(ct);
        return settings.ToDictionary(s => s.Key);
    }

    private static bool ReadBoolean(IReadOnlyDictionary<SystemSettingKey, SystemSetting> byKey, SystemSettingKey key)
    {
        return byKey.TryGetValue(key, out var setting) ? setting.AsBoolean() : MissingSettingFallback;
    }

    /// <summary>
    /// Updates the existing row, or materialises one when the key has never been persisted, so a
    /// database seeded before a key existed heals on the first save instead of silently dropping it.
    /// </summary>
    private static SystemSetting Apply(
        IReadOnlyDictionary<SystemSettingKey, SystemSetting> byKey,
        SystemSettingKey key,
        bool value)
    {
        if (!byKey.TryGetValue(key, out var setting))
        {
            return SystemSetting.CreateBoolean(key, value);
        }

        setting.UpdateBoolean(value);
        return setting;
    }
}
