using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface ISubmissionRepository
{
    Task<List<Submission>> GetAllAsync(CancellationToken ct = default);
}
