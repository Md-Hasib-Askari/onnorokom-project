using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class AssignmentRepository(AppDbContext dbContext) : IAssignmentRepository
{
    public async Task<List<Assignment>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Assignments
            .Include(a => a.Subject)
            .ThenInclude(s => s!.Grade)
            .Include(a => a.Teacher)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Assignments
            .Include(a => a.Subject)
            .ThenInclude(s => s!.Grade)
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task AddAsync(Assignment assignment, CancellationToken ct = default)
    {
        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Assignment assignment, CancellationToken ct = default)
    {
        dbContext.Assignments.Update(assignment);
        await dbContext.SaveChangesAsync(ct);
    }
}
