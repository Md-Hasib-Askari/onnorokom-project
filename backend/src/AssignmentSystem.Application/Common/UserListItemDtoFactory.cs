using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Common;

public static class UserListItemDtoFactory
{
    public static async Task<List<UserListItemDto>> BuildAsync(
        IReadOnlyCollection<AuthUser> users,
        IProfileRepository profileRepository,
        IGradeRepository gradeRepository,
        CancellationToken ct)
    {
        var studentUserIds = users.Where(u => u.Role == UserRole.Student).Select(u => u.Id).ToList();
        var studentProfiles = await profileRepository.GetStudentsByUserIdsAsync(studentUserIds, ct);
        var profileByUserId = studentProfiles.ToDictionary(p => p.AuthUserId);

        var gradeIds = studentProfiles.Select(p => p.GradeId).Distinct().ToList();
        var grades = await gradeRepository.GetByIdsAsync(gradeIds, ct);
        var gradeById = grades.ToDictionary(g => g.Id);

        var dtos = new List<UserListItemDto>(users.Count);
        foreach (var user in users)
        {
            Guid? gradeId = null;
            string? gradeName = null;

            if (profileByUserId.TryGetValue(user.Id, out var studentProfile))
            {
                gradeId = studentProfile.GradeId;
                gradeName = gradeById.TryGetValue(studentProfile.GradeId, out var grade) ? grade.Name : null;
            }

            dtos.Add(new UserListItemDto(
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.Status,
                user.CreatedAt,
                user.IsActive,
                gradeId,
                gradeName));
        }

        return dtos;
    }
}