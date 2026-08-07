using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Assignments;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class AdminQueryService(
    IAssignmentRepository assignmentRepository,
    ISubmissionRepository submissionRepository,
    IMapper mapper) : IAdminQueryService
{
    public async Task<List<AssignmentListItemDto>> GetAllAssignmentsAsync(CancellationToken ct = default)
    {
        var assignments = await assignmentRepository.GetAllAsync(ct);
        return mapper.Map<List<AssignmentListItemDto>>(assignments);
    }

    public async Task<List<SubmissionListItemDto>> GetAllSubmissionsAsync(CancellationToken ct = default)
    {
        var submissions = await submissionRepository.GetAllAsync(ct);
        return mapper.Map<List<SubmissionListItemDto>>(submissions);
    }
}
