using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Sections;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class SectionRepository(AppDbContext dbContext) : ISectionRepository
{
    public async Task<List<Section>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Sections
            .Include(s => s.Grade)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Section?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Sections
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <summary>
    /// Teacher and student totals per section for the admin section list. Teachers are the
    /// distinct teachers of the section's section-subject links; students are the profiles
    /// enrolled in the section. Soft-deleted rows are excluded by the global query filter.
    /// </summary>
    public async Task<Dictionary<Guid, SectionCounts>> GetCountsAsync(CancellationToken ct = default)
    {
        var teacherCounts = await dbContext.SectionSubjects
            .Where(ss => ss.TeacherId != null)
            .GroupBy(ss => ss.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Select(ss => ss.TeacherId).Distinct().Count() })
            .ToListAsync(ct);

        var studentCounts = await dbContext.StudentProfiles
            .GroupBy(sp => sp.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var counts = new Dictionary<Guid, SectionCounts>();
        foreach (var row in teacherCounts)
        {
            counts[row.SectionId] = new SectionCounts(row.Count, 0);
        }

        foreach (var row in studentCounts)
        {
            counts[row.SectionId] = counts.TryGetValue(row.SectionId, out var existing)
                ? existing with { StudentCount = row.Count }
                : new SectionCounts(0, row.Count);
        }

        return counts;
    }

    public async Task<List<Section>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await dbContext.Sections
            .Include(s => s.Grade)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Sections.AnyAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
    {
        return await dbContext.Sections.AnyAsync(s => s.Name == name && s.GradeId == gradeId, ct);
    }

    public async Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.StudentProfiles.AnyAsync(s => s.SectionId == id, ct);
    }

    public async Task AddAsync(Section section, CancellationToken ct = default)
    {
        dbContext.Sections.Add(section);
        await SaveAsync(section.Name, section.GradeId, ct);
    }

    public async Task UpdateAsync(Section section, CancellationToken ct = default)
    {
        dbContext.Sections.Update(section);
        await SaveAsync(section.Name, section.GradeId, ct);
    }

    private async Task SaveAsync(string name, Guid gradeId, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Only reachable when a concurrent write slips past SectionService's own uniqueness
            // check, so word it the same way that check does rather than exposing the grade id.
            var gradeLabel = await dbContext.GetGradeLabelAsync(gradeId, ct);
            throw new DuplicateEntityException($"Section '{name}' in {gradeLabel} already exists.");
        }
    }
}
