using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class AssignmentRepository(AppDbContext dbContext) : IAssignmentRepository
{
    public async Task<PagedResult<Assignment>> GetPageAsync(
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        CancellationToken ct = default)
    {
        var rows = await WithDetails()
            .ApplyKeysetPaging(a => a.CreatedAt, afterCreatedAt, afterId, descending: true, limit)
            .ToListAsync(ct);

        return PagedResult<Assignment>.FromRows(rows, limit, last => CursorCodec.Encode(last.CreatedAt, last.Id));
    }

    public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await WithDetails().FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<PagedResult<Assignment>> GetPageByTeacherAsync(
        Guid teacherId,
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        CancellationToken ct = default)
    {
        var rows = await WithDetails()
            .Where(a => a.TeacherId == teacherId)
            .ApplyKeysetPaging(a => a.CreatedAt, afterCreatedAt, afterId, descending: true, limit)
            .ToListAsync(ct);

        return PagedResult<Assignment>.FromRows(rows, limit, last => CursorCodec.Encode(last.CreatedAt, last.Id));
    }

    public async Task<PagedResult<Assignment>> GetPublishedPageForSectionAsync(
        Guid sectionId,
        int limit,
        DateTimeOffset? afterCreatedAt,
        Guid? afterId,
        CancellationToken ct = default)
    {
        var rows = await WithDetails()
            .Where(a => a.SectionId == sectionId && a.Status == AssignmentStatus.Published)
            .ApplyKeysetPaging(a => a.CreatedAt, afterCreatedAt, afterId, descending: true, limit)
            .ToListAsync(ct);

        return PagedResult<Assignment>.FromRows(rows, limit, last => CursorCodec.Encode(last.CreatedAt, last.Id));
    }

    public async Task<bool> HasSubmissionsAsync(Guid assignmentId, CancellationToken ct = default)
    {
        return await dbContext.Submissions.AnyAsync(s => s.AssignmentId == assignmentId, ct);
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

    public async Task DeleteAsync(Assignment assignment, CancellationToken ct = default)
    {
        assignment.Delete();
        dbContext.Assignments.Update(assignment);
        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Grade reads through <c>Section</c> now that assignments are section-scoped, but
    /// <c>Subject.Grade</c> stays included because the admin list still labels rows from it.
    /// </summary>
    private IQueryable<Assignment> WithDetails()
    {
        return dbContext.Assignments
            .Include(a => a.Section)
            .ThenInclude(s => s!.Grade)
            .Include(a => a.Subject)
            .ThenInclude(s => s!.Grade)
            .Include(a => a.Teacher);
    }
}