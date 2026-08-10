using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

internal static class AppDbContextExtensions
{
    /// <summary>Stand-in when the grade is gone by the time the message is built.</summary>
    private const string UnknownGradeLabel = "that grade";

    /// <summary>
    /// Resolves a grade's display name for error messages, so a duplicate-name conflict reads
    /// "in Grade 6" instead of leaking the raw grade id past the API boundary.
    /// </summary>
    public static async Task<string> GetGradeLabelAsync(
        this AppDbContext dbContext,
        Guid gradeId,
        CancellationToken ct)
    {
        var name = await dbContext.Grades
            .Where(g => g.Id == gradeId)
            .Select(g => g.Name)
            .FirstOrDefaultAsync(ct);

        return name ?? UnknownGradeLabel;
    }
}