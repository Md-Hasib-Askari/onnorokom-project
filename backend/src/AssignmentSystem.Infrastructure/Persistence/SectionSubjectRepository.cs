using System.Linq.Expressions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class SectionSubjectRepository(AppDbContext dbContext) : ISectionSubjectRepository
{
    public async Task<List<SectionSubject>> GetBySectionAsync(Guid sectionId, CancellationToken ct = default)
    {
        return await dbContext.SectionSubjects
            .Include(ss => ss.Subject)
            .Include(ss => ss.Teacher)
            .Where(ss => ss.SectionId == sectionId)
            .ToListAsync(ct);
    }

    public async Task<SectionSubject?> GetBySectionAndSubjectAsync(Guid sectionId, Guid subjectId, CancellationToken ct = default)
    {
        return await dbContext.SectionSubjects
            .Include(ss => ss.Subject)
            .Include(ss => ss.Teacher)
            .FirstOrDefaultAsync(ss => ss.SectionId == sectionId && ss.SubjectId == subjectId, ct);
    }

    public async Task AddAsync(SectionSubject sectionSubject, CancellationToken ct = default)
    {
        dbContext.SectionSubjects.Add(sectionSubject);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SectionSubject sectionSubject, CancellationToken ct = default)
    {
        dbContext.SectionSubjects.Update(sectionSubject);
        await dbContext.SaveChangesAsync(ct);
    }

    public Task SoftDeleteForSectionAsync(Guid sectionId, CancellationToken ct = default)
        => SoftDeleteWhereAsync(ss => ss.SectionId == sectionId, ct);

    public Task SoftDeleteForSubjectAsync(Guid subjectId, CancellationToken ct = default)
        => SoftDeleteWhereAsync(ss => ss.SubjectId == subjectId, ct);

    /// <summary>
    /// Soft-deletes the matching links so they stop counting towards "is this teacher still in
    /// use?" once their section or subject is gone. Without this they linger as live rows
    /// pointing at a deleted parent and permanently block deleting the teacher.
    /// </summary>
    private async Task SoftDeleteWhereAsync(
        Expression<Func<SectionSubject, bool>> predicate,
        CancellationToken ct)
    {
        var links = await dbContext.SectionSubjects.Where(predicate).ToListAsync(ct);
        foreach (var link in links)
        {
            link.Delete();
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
