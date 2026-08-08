namespace AssignmentSystem.Application.Common.Interfaces;

public interface ITransactionService
{
    Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
}
