using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class SubjectRepository(AppDbContext dbContext) : ISubjectRepository
{
    public async Task<List<Subject>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Subjects
            .Include(s => s.Grade)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Subjects
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <summary>
    /// Distinct teachers per subject across every section-subject link, for the admin subject
    /// list. Soft-deleted links are excluded by the global query filter.
    /// </summary>
    public async Task<Dictionary<Guid, int>> GetTeacherCountsAsync(CancellationToken ct = default)
    {
        return await dbContext.SectionSubjects
            .Where(ss => ss.TeacherId != null)
            .GroupBy(ss => ss.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Select(ss => ss.TeacherId).Distinct().Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Subjects.AnyAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
    {
        return await dbContext.Subjects.AnyAsync(s => s.Name == name && s.GradeId == gradeId, ct);
    }

    public async Task<bool> HasAssignmentsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Assignments.AnyAsync(a => a.SubjectId == id, ct);
    }

    public Task AddAsync(Subject subject, CancellationToken ct = default)
    {
        dbContext.Subjects.Add(subject);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Subject subject, CancellationToken ct = default)
    {
        dbContext.Subjects.Update(subject);
        return Task.CompletedTask;
    }
}
