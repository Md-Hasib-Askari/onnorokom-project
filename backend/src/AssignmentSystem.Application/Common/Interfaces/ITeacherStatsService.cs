using AssignmentSystem.Application.DTOs.Teacher;

namespace AssignmentSystem.Application.Common.Interfaces;

/// <summary>Counts backing the teacher overview page.</summary>
public interface ITeacherStatsService
{
    Task<TeacherOverviewDto> GetOverviewAsync(CancellationToken ct = default);
}
