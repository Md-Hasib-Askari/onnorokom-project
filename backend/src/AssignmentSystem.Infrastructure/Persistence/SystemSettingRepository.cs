using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class SystemSettingRepository(AppDbContext dbContext) : ISystemSettingRepository
{
    public async Task<List<SystemSetting>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.SystemSettings
            .OrderBy(s => s.Key)
            .ToListAsync(ct);
    }

    public async Task<SystemSetting?> GetByKeyAsync(SystemSettingKey key, CancellationToken ct = default)
    {
        return await dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
    }

    /// <summary>
    /// Writes a batch of settings in one round trip. Rows loaded through this context are already
    /// tracked, so their edits flow out on save; a detached entity is one the service materialised
    /// for a key that has no row yet and therefore needs inserting rather than updating.
    /// </summary>
    public Task UpsertAsync(IReadOnlyCollection<SystemSetting> settings, CancellationToken ct = default)
    {
        foreach (var setting in settings)
        {
            if (dbContext.Entry(setting).State == EntityState.Detached)
            {
                dbContext.SystemSettings.Add(setting);
            }
        }

        return Task.CompletedTask;
    }
}
