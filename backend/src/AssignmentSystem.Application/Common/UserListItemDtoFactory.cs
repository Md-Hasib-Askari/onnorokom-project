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
        ISectionRepository sectionRepository,
        CancellationToken ct)
    {
        var studentUserIds = users
            .Where(u => u.Role == UserRole.Student)
            .Select(u => u.Id)
            .ToList();
        var studentProfiles = await profileRepository.GetStudentsByUserIdsAsync(studentUserIds, ct);
        var profileByUserId = studentProfiles.ToDictionary(p => p.AuthUserId);

        var sectionIds = studentProfiles.Select(p => p.SectionId).Distinct().ToList();
        var sections = await sectionRepository.GetByIdsAsync(sectionIds, ct);
        var sectionById = sections.ToDictionary(s => s.Id);

        var teacherUserIds = users
            .Where(u => u.Role == UserRole.Teacher)
            .Select(u => u.Id)
            .ToList();
        var teacherProfiles = await profileRepository.GetTeachersByUserIdsAsync(teacherUserIds, ct);
        var teacherProfileByUserId = teacherProfiles.ToDictionary(p => p.AuthUserId);

        var dtos = new List<UserListItemDto>(users.Count);
        foreach (var user in users)
        {
            Guid? sectionId = null;
            string? sectionName = null;
            string? gradeName = null;

            if (profileByUserId.TryGetValue(user.Id, out var studentProfile))
            {
                sectionId = studentProfile.SectionId;
                if (sectionById.TryGetValue(studentProfile.SectionId, out var section))
                {
                    sectionName = section.Name;
                    gradeName = section.Grade?.Name;
                }
            }

            string? teacherCode = null;
            if (teacherProfileByUserId.TryGetValue(user.Id, out var teacherProfile))
            {
                teacherCode = teacherProfile.TeacherCode;
            }

            dtos.Add(new UserListItemDto(
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.Status,
                user.CreatedAt,
                user.IsActive,
                sectionId,
                sectionName,
                gradeName,
                teacherCode));
        }

        return dtos;
    }
}
