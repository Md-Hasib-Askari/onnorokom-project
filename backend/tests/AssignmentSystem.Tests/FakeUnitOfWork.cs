using AssignmentSystem.Application.Common.Interfaces;

namespace AssignmentSystem.Tests;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task SaveAsync(CancellationToken ct = default) => Task.CompletedTask;
}