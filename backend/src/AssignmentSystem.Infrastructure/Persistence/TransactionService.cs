using AssignmentSystem.Application.Common.Interfaces;

namespace AssignmentSystem.Infrastructure.Persistence;

public class TransactionService(AppDbContext dbContext) : ITransactionService
{
    public async Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            await work(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
