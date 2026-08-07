using AssignmentSystem.Application.Common.Interfaces;
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

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Grades.AnyAsync(g => g.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(string name, string academicYear, CancellationToken ct = default)
    {
        return await dbContext.Grades.AnyAsync(g => g.Name == name && g.AcademicYear == academicYear, ct);
    }

    public async Task<bool> HasSubjectsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Subjects.AnyAsync(s => s.GradeId == id, ct);
    }

    public async Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.StudentProfiles.AnyAsync(s => s.GradeId == id, ct);
    }

    public async Task AddAsync(Grade grade, CancellationToken ct = default)
    {
        dbContext.Grades.Add(grade);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Grade grade, CancellationToken ct = default)
    {
        dbContext.Grades.Update(grade);
        await dbContext.SaveChangesAsync(ct);
    }
}
