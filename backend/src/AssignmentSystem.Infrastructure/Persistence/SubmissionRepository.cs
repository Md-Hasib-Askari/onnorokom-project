using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Assignments;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
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

    public async Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        return await dbContext.Submissions
            .Include(s => s.Student)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderBy(s => s.Student!.FullName)
            .ToListAsync(ct);
    }

    public async Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default)
    {
        return await dbContext.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, ct);
    }

    public async Task<List<Submission>> GetByStudentAndAssignmentIdsAsync(
        Guid studentId,
        IEnumerable<Guid> assignmentIds,
        CancellationToken ct = default)
    {
        var ids = assignmentIds.ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.Submissions
            .Where(s => s.StudentId == studentId && ids.Contains(s.AssignmentId))
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, SubmissionCounts>> GetCountsByAssignmentIdsAsync(
        IEnumerable<Guid> assignmentIds,
        CancellationToken ct = default)
    {
        var ids = assignmentIds.ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var rows = await dbContext.Submissions
            .Where(s => ids.Contains(s.AssignmentId))
            .GroupBy(s => s.AssignmentId)
            .Select(g => new
            {
                AssignmentId = g.Key,
                Total = g.Count(),
                Graded = g.Count(s => s.Status == SubmissionStatus.Graded)
            })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.AssignmentId, r => new SubmissionCounts(r.Total, r.Graded));
    }

    public async Task<decimal?> GetMaxAwardedMarksAsync(Guid assignmentId, CancellationToken ct = default)
    {
        return await dbContext.Submissions
            .Where(s => s.AssignmentId == assignmentId && s.Marks != null)
            .MaxAsync(s => s.Marks, ct);
    }

    public async Task AddAsync(Submission submission, CancellationToken ct = default)
    {
        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Submission submission, CancellationToken ct = default)
    {
        dbContext.Submissions.Update(submission);
        await dbContext.SaveChangesAsync(ct);
    }
}