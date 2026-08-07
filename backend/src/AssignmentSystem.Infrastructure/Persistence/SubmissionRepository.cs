using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class SubmissionRepository(AppDbContext dbContext) : ISubmissionRepository
{
    public async Task<List<Submission>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Submissions
            .Include(s => s.Assignment)
            .ThenInclude(a => a!.Subject)
            .ThenInclude(sub => sub!.Grade)
            .Include(s => s.Student)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(ct);
    }
}
