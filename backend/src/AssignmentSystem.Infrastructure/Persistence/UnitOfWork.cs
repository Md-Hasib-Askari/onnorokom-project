using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task SaveAsync(CancellationToken ct = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw await TranslateUniqueViolationAsync(ex, ct);
        }
    }

    /// <summary>
    /// Maps a database unique violation to the domain exception the pre-check would have thrown, so
    /// a concurrent write that slipped past the service's own uniqueness check surfaces the same
    /// 409 (or duplicate-email) error instead of a raw 500.
    /// </summary>
    private async Task<DuplicateEntityException> TranslateUniqueViolationAsync(DbUpdateException ex, CancellationToken ct)
    {
        foreach (var entry in ex.Entries)
        {
            switch (entry.Entity)
            {
                case AuthUser user:
                    return new DuplicateEmailException(user.Email);
                case Grade grade:
                    return new DuplicateEntityException($"Grade '{grade.Name}' for academic year {grade.AcademicYear} already exists.");
                case Section section:
                    var sectionGrade = await dbContext.GetGradeLabelAsync(section.GradeId, ct);
                    return new DuplicateEntityException($"Section '{section.Name}' in {sectionGrade} already exists.");
                case Subject subject:
                    var subjectGrade = await dbContext.GetGradeLabelAsync(subject.GradeId, ct);
                    return new DuplicateEntityException($"Subject '{subject.Name}' in {subjectGrade} already exists.");
            }
        }

        return new DuplicateEntityException("A duplicate value was rejected by the database.");
    }
}