using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISystemSettingRepository
{
    Task<List<SystemSetting>> GetAllAsync(CancellationToken ct = default);
    Task<SystemSetting?> GetByKeyAsync(SystemSettingKey key, CancellationToken ct = default);
    Task UpsertAsync(IReadOnlyCollection<SystemSetting> settings, CancellationToken ct = default);
}
