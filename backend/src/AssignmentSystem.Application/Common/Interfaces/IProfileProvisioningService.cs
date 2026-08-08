using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Common.Interfaces;

public interface IProfileProvisioningService
{
    Task CreateProfileAsync(AuthUser user, Guid? studentGradeId, CancellationToken ct = default);
}
