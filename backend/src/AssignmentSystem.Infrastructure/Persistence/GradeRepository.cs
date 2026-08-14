using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Grades;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class GradeRepository(AppDbContext dbContext) : IGradeRepository
{
    public async Task<List<Grade>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Grades
            .OrderBy(g => g.Name)
            .ToListAsync(ct);
    }

    public async Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Grades.FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    /// <summary>
    /// Teacher and student totals per grade. A grade's teachers are the distinct teachers of any
    /// section-subject link inside its sections; its students are the profiles enrolled in those
    /// sections. Soft-deleted rows are excluded by the global query filter.
    /// </summary>
    public async Task<Dictionary<Guid, GradeCounts>> GetCountsAsync(CancellationToken ct = default)
    {
        var teacherCounts = await dbContext.SectionSubjects
            .Where(ss => ss.TeacherId != null)
            .GroupBy(ss => ss.Section!.GradeId)
            .Select(g => new { GradeId = g.Key, Count = g.Select(ss => ss.TeacherId).Distinct().Count() })
            .ToListAsync(ct);

        var studentCounts = await dbContext.StudentProfiles
            .GroupBy(sp => sp.Section!.GradeId)
            .Select(g => new { GradeId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var counts = new Dictionary<Guid, GradeCounts>();
        foreach (var row in teacherCounts)
        {
            counts[row.GradeId] = new GradeCounts(row.Count, 0);
        }

        foreach (var row in studentCounts)
        {
            counts[row.GradeId] = counts.TryGetValue(row.GradeId, out var existing)
                ? existing with { StudentCount = row.Count }
                : new GradeCounts(0, row.Count);
        }

        return counts;
    }

    public async Task<bool> ExistsAsync(string name, string academicYear, CancellationToken ct = default)
    {
        return await dbContext.Grades.AnyAsync(g => g.Name == name && g.AcademicYear == academicYear, ct);
    }

    public async Task<bool> HasSubjectsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Subjects.AnyAsync(s => s.GradeId == id, ct);
    }

    public async Task<bool> HasSectionsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Sections.AnyAsync(s => s.GradeId == id, ct);
    }

    public async Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.StudentProfiles.AnyAsync(s => s.Section!.GradeId == id, ct);
    }

    public Task AddAsync(Grade grade, CancellationToken ct = default)
    {
        dbContext.Grades.Add(grade);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Grade grade, CancellationToken ct = default)
    {
        dbContext.Grades.Update(grade);
        return Task.CompletedTask;
    }
}
