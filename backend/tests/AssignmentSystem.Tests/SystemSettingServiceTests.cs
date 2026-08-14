using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Settings;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class SystemSettingServiceTests
{
    private readonly FakeSystemSettingRepository _repo = new();
    private readonly SystemSettingService _sut;

    public SystemSettingServiceTests()
    {
        _sut = new SystemSettingService(_repo, new FakeUnitOfWork());
    }

    [Fact]
    public async Task GetRegistrationPolicy_ReturnsStoredValues()
    {
        Seed(teacherRegistration: true, studentRegistration: false);

        var policy = await _sut.GetRegistrationPolicyAsync();

        Assert.True(policy.TeacherSelfRegistrationEnabled);
        Assert.False(policy.StudentSelfRegistrationEnabled);
    }

    /// <summary>
    /// A database that missed the seed must close registration rather than open a role the admin
    /// never enabled.
    /// </summary>
    [Fact]
    public async Task GetRegistrationPolicy_MissingKeys_FallsBackToClosed()
    {
        var policy = await _sut.GetRegistrationPolicyAsync();

        Assert.False(policy.TeacherSelfRegistrationEnabled);
        Assert.False(policy.StudentSelfRegistrationEnabled);
    }

    [Fact]
    public async Task GetRegistrationPolicy_UnparseableValue_ReadsAsFalse()
    {
        _repo.Settings.Add(
            WithRawValue(SystemSettingKey.TeacherSelfRegistrationEnabled, "yes-please"));

        var policy = await _sut.GetRegistrationPolicyAsync();

        Assert.False(policy.TeacherSelfRegistrationEnabled);
    }

    [Fact]
    public async Task GetSystemSettings_ReturnsBothPolicies()
    {
        Seed(
            teacherRegistration: true,
            studentRegistration: false,
            teacherProfileEdit: false,
            studentProfileEdit: true);

        var settings = await _sut.GetSystemSettingsAsync();

        Assert.True(settings.TeacherSelfRegistrationEnabled);
        Assert.False(settings.StudentSelfRegistrationEnabled);
        Assert.False(settings.TeacherProfileSelfEditEnabled);
        Assert.True(settings.StudentProfileSelfEditEnabled);
    }

    [Fact]
    public async Task UpdateSystemSettings_OverwritesExistingRows()
    {
        Seed(
            teacherRegistration: true,
            studentRegistration: false,
            teacherProfileEdit: false,
            studentProfileEdit: true);

        var settings = await _sut.UpdateSystemSettingsAsync(
            new SystemSettingsUpdateRequest(false, true, true, false));

        Assert.False(settings.TeacherSelfRegistrationEnabled);
        Assert.True(settings.StudentSelfRegistrationEnabled);
        Assert.True(settings.TeacherProfileSelfEditEnabled);
        Assert.False(settings.StudentProfileSelfEditEnabled);

        var reloaded = await _sut.GetSystemSettingsAsync();
        Assert.False(reloaded.TeacherSelfRegistrationEnabled);
        Assert.True(reloaded.StudentSelfRegistrationEnabled);
        Assert.True(reloaded.TeacherProfileSelfEditEnabled);
        Assert.False(reloaded.StudentProfileSelfEditEnabled);
    }

    /// <summary>
    /// A key that was never persisted heals on the first save instead of being silently dropped.
    /// </summary>
    [Fact]
    public async Task UpdateSystemSettings_MissingRows_InsertsThem()
    {
        await _sut.UpdateSystemSettingsAsync(
            new SystemSettingsUpdateRequest(true, true, true, true));

        Assert.Equal(4, _repo.Settings.Count);

        var reloaded = await _sut.GetSystemSettingsAsync();
        Assert.True(reloaded.TeacherSelfRegistrationEnabled);
        Assert.True(reloaded.StudentSelfRegistrationEnabled);
        Assert.True(reloaded.TeacherProfileSelfEditEnabled);
        Assert.True(reloaded.StudentProfileSelfEditEnabled);
    }

    [Theory]
    [InlineData(UserRole.Teacher)]
    [InlineData(UserRole.Student)]
    public async Task EnsureSelfRegistrationAllowed_RoleEnabled_DoesNotThrow(UserRole role)
    {
        Seed();

        await _sut.EnsureSelfRegistrationAllowedAsync(role);
    }

    [Theory]
    [InlineData(UserRole.Teacher)]
    [InlineData(UserRole.Student)]
    public async Task EnsureSelfRegistrationAllowed_RoleDisabled_Throws(UserRole role)
    {
        Seed(teacherRegistration: false, studentRegistration: false, teacherProfileEdit: false, studentProfileEdit: false);

        await Assert.ThrowsAsync<RegistrationDisabledException>(
            () => _sut.EnsureSelfRegistrationAllowedAsync(role));
    }

    /// <summary>No setting can open the Admin role to public sign-up.</summary>
    [Fact]
    public async Task EnsureSelfRegistrationAllowed_Admin_AlwaysThrows()
    {
        Seed();

        await Assert.ThrowsAsync<RegistrationDisabledException>(
            () => _sut.EnsureSelfRegistrationAllowedAsync(UserRole.Admin));
    }

    /// <summary>
    /// Builds a setting holding text no boolean parser accepts. The entity deliberately offers no
    /// way to write such a value, so this reaches past the setter to reproduce the one thing that
    /// can produce it: a row edited by hand in the database.
    /// </summary>
    private static SystemSetting WithRawValue(SystemSettingKey key, string rawValue)
    {
        var setting = SystemSetting.CreateBoolean(key, true);

        typeof(SystemSetting)
            .GetProperty(nameof(SystemSetting.Value))!
            .SetValue(setting, rawValue);

        return setting;
    }

    private void Seed(
        bool teacherRegistration = true,
        bool studentRegistration = true,
        bool teacherProfileEdit = true,
        bool studentProfileEdit = true)
    {
        _repo.Settings.Add(
            SystemSetting.CreateBoolean(SystemSettingKey.TeacherSelfRegistrationEnabled, teacherRegistration));
        _repo.Settings.Add(
            SystemSetting.CreateBoolean(SystemSettingKey.StudentSelfRegistrationEnabled, studentRegistration));
        _repo.Settings.Add(
            SystemSetting.CreateBoolean(SystemSettingKey.TeacherProfileSelfEditEnabled, teacherProfileEdit));
        _repo.Settings.Add(
            SystemSetting.CreateBoolean(SystemSettingKey.StudentProfileSelfEditEnabled, studentProfileEdit));
    }

    /// <summary>
    /// Stands in for EF change tracking: entities the service mutated are the same instances the
    /// list already holds, so only the ones it materialised need adding.
    /// </summary>
    private sealed class FakeSystemSettingRepository : ISystemSettingRepository
    {
        public List<SystemSetting> Settings { get; } = [];

        public Task<List<SystemSetting>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Settings.ToList());

        public Task<SystemSetting?> GetByKeyAsync(SystemSettingKey key, CancellationToken ct = default)
            => Task.FromResult(Settings.FirstOrDefault(s => s.Key == key));

        public Task UpsertAsync(IReadOnlyCollection<SystemSetting> settings, CancellationToken ct = default)
        {
            foreach (var setting in settings.Where(s => !Settings.Contains(s)))
            {
                Settings.Add(setting);
            }

            return Task.CompletedTask;
        }
    }
}