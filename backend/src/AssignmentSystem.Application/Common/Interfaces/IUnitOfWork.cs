namespace AssignmentSystem.Application.Common.Interfaces;

/// <summary>
/// The single point where all pending repository changes are flushed to the database. Repositories
/// only track entities; services call <see cref="SaveAsync"/> once at the end of a write operation,
/// which is atomic because EF wraps one <c>SaveChanges</c> in its own transaction.
/// </summary>
public interface IUnitOfWork
{
    Task SaveAsync(CancellationToken ct = default);
}