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
            .Include(s => s.Teacher)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Subjects
            .Include(s => s.Grade)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
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

    public async Task AddAsync(Subject subject, CancellationToken ct = default)
    {
        dbContext.Subjects.Add(subject);
        await SaveAsync(subject.Name, subject.GradeId, ct);
    }

    public async Task UpdateAsync(Subject subject, CancellationToken ct = default)
    {
        dbContext.Subjects.Update(subject);
        await SaveAsync(subject.Name, subject.GradeId, ct);
    }

    private async Task SaveAsync(string name, Guid gradeId, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw new DuplicateEntityException($"Subject '{name}' in grade {gradeId} already exists.");
        }
    }
}
