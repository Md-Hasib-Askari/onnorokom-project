using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AutoMapper;

namespace AssignmentSystem.Application.Services;

public class AdminUserService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IGradeRepository gradeRepository,
    IPasswordHasher passwordHasher,
    IMapper mapper) : IAdminUserService
{
    public async Task<List<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        return mapper.Map<List<UserListItemDto>>(users);
    }

    public async Task<UserListItemDto> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DuplicateEmailException(email);
        }

        if (request.Role == UserRole.Student && request.StudentGradeId is null)
        {
            throw new DomainException("A grade is required for student users.");
        }

        if (request.Role == UserRole.Student
            && request.StudentGradeId is not null
            && !await gradeRepository.ExistsAsync(request.StudentGradeId.Value, ct))
        {
            throw new EntityNotFoundException($"Grade with id {request.StudentGradeId} was not found.");
        }

        var user = AuthUser.CreatePending(request.FullName, email, passwordHasher.Hash(request.Password), request.Role);
        user.Approve();
        await userRepository.AddAsync(user, ct);

        await CreateProfileAsync(user, request.StudentGradeId, ct);

        return mapper.Map<UserListItemDto>(user);
    }

    public async Task<UserListItemDto> UpdateUserAsync(Guid userId, UserUpdateRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (email != user.Email && await userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DuplicateEmailException(email);
        }

        if (user.Role == UserRole.Student && request.StudentGradeId is null)
        {
            throw new DomainException("A grade is required for student users.");
        }

        if (user.Role == UserRole.Student
            && request.StudentGradeId is not null
            && !await gradeRepository.ExistsAsync(request.StudentGradeId.Value, ct))
        {
            throw new EntityNotFoundException($"Grade with id {request.StudentGradeId} was not found.");
        }

        if (request.Status == AccountStatus.Pending)
        {
            throw new DomainException("Pending cannot be set via user update; use the approval endpoint.");
        }

        user.UpdateDetails(request.FullName, email);

        if (request.Status == AccountStatus.Approved)
        {
            user.Approve();
        }
        else if (request.Status == AccountStatus.Rejected)
        {
            user.Reject();
        }

        if (request.IsActive)
        {
            user.Reactivate();
        }
        else
        {
            user.Deactivate();
        }

        await userRepository.UpdateAsync(user, ct);
        await UpdateStudentProfileAsync(user, request.StudentGradeId, ct);
        return mapper.Map<UserListItemDto>(user);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        if (await IsInUseAsync(user, ct))
        {
            throw new EntityInUseException($"User '{user.FullName}' cannot be deleted because they are referenced by existing records.");
        }

        user.Delete();
        await userRepository.UpdateAsync(user, ct);
    }

    private async Task<bool> IsInUseAsync(AuthUser user, CancellationToken ct)
    {
        return user.Role switch
        {
            UserRole.Teacher => await userRepository.HasAssignedSubjectsAsync(user.Id, ct)
                || await userRepository.HasAssignmentsAsync(user.Id, ct)
                || await userRepository.HasGradedSubmissionsAsync(user.Id, ct),
            UserRole.Student => await userRepository.HasSubmissionsAsync(user.Id, ct),
            _ => false
        };
    }

    private async Task CreateProfileAsync(AuthUser user, Guid? studentGradeId, CancellationToken ct)
    {
        switch (user.Role)
        {
            case UserRole.Teacher:
                await profileRepository.AddAsync(TeacherProfile.Create(user.Id), ct);
                break;
            case UserRole.Student:
                await profileRepository.AddAsync(StudentProfile.Create(user.Id, studentGradeId!.Value), ct);
                break;
        }
    }

    private async Task UpdateStudentProfileAsync(AuthUser user, Guid? studentGradeId, CancellationToken ct)
    {
        if (user.Role != UserRole.Student)
        {
            return;
        }

        var studentProfile = await profileRepository.GetStudentByUserIdAsync(user.Id, ct);
        if (studentProfile is null)
        {
            await profileRepository.AddAsync(StudentProfile.Create(user.Id, studentGradeId!.Value), ct);
        }
        else
        {
            studentProfile.ChangeGrade(studentGradeId!.Value);
            await profileRepository.UpdateAsync(studentProfile, ct);
        }
    }
}
