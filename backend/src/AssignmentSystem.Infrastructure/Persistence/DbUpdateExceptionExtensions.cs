using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AssignmentSystem.Infrastructure.Persistence;

internal static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueViolation(this DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
