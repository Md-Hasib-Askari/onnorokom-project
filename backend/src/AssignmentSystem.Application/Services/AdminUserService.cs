using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class AdminUserService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IGradeRepository gradeRepository,
    IPasswordHasher passwordHasher,
    ITransactionService transactionService,
    ICurrentUser currentUser,
    IProfileProvisioningService profileProvisioningService) : IAdminUserService
{
    public async Task<List<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        return await UserListItemDtoFactory.BuildAsync(users, profileRepository, gradeRepository, ct);
    }

    private async Task<UserListItemDto> BuildDtoAsync(AuthUser user, CancellationToken ct)
    {
        Guid? gradeId = null;
        string? gradeName = null;

        if (user.Role == UserRole.Student)
        {
            var studentProfile = await profileRepository.GetStudentByUserIdAsync(user.Id, ct);
            if (studentProfile is not null)
            {
                gradeId = studentProfile.GradeId;
                var grade = await gradeRepository.GetByIdAsync(studentProfile.GradeId, ct);
                gradeName = grade?.Name;
            }
        }

        return new UserListItemDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status,
            user.CreatedAt,
            user.IsActive,
            gradeId,
            gradeName);
    }

    public async Task<UserListItemDto> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DuplicateEmailException(email);
        }

        await UserGuards.EnsureStudentGradeValidAsync(gradeRepository, request.Role, request.StudentGradeId, ct);

        var user = AuthUser.CreatePending(request.FullName, email, passwordHasher.Hash(request.Password), request.Role);
        user.Approve();
        await transactionService.ExecuteAsync(async transactionCt =>
        {
            await userRepository.AddAsync(user, transactionCt);
            await profileProvisioningService.CreateProfileAsync(user, request.StudentGradeId, transactionCt);
        }, ct);

        return await BuildDtoAsync(user, ct);
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

        await UserGuards.EnsureStudentGradeValidAsync(gradeRepository, user.Role, request.StudentGradeId, ct);

        if (request.Status == AccountStatus.Pending)
        {
            throw new DomainException("Pending cannot be set via user update; use the approval endpoint.");
        }

        var willBecomeUnusableAdmin = user.IsUsableAdmin
            && (request.Status == AccountStatus.Rejected
                || (request.Status == AccountStatus.Approved && !request.IsActive));

        await UserGuards.EnsureNotLastUsableAdminAsync(
            userRepository,
            willBecomeUnusableAdmin,
            "The last admin account cannot be deactivated or rejected.",
            ct);

        user.UpdateDetails(request.FullName, email);
        user.ApplyStatus(request.Status, request.IsActive);

        await transactionService.ExecuteAsync(async transactionCt =>
        {
            await userRepository.UpdateAsync(user, transactionCt);
            await UpdateProfileAsync(user, request, transactionCt);
        }, ct);
        return await BuildDtoAsync(user, ct);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        if (currentUser.UserId is not null && user.Id.ToString() == currentUser.UserId)
        {
            throw new DomainException("You cannot delete your own account.");
        }

        await UserGuards.EnsureNotLastUsableAdminAsync(
            userRepository,
            user.IsUsableAdmin,
            "The last admin account cannot be deleted.",
            ct);

        if (await IsInUseAsync(user, ct))
        {
            throw new EntityInUseException($"User '{user.FullName}' cannot be deleted because they are referenced by existing records.");
        }

        await transactionService.ExecuteAsync(async transactionCt =>
        {
            user.Delete();
            await userRepository.UpdateAsync(user, transactionCt);
            await profileRepository.SoftDeleteForUserAsync(user.Id, transactionCt);
        }, ct);
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

    private async Task UpdateProfileAsync(AuthUser user, UserUpdateRequest request, CancellationToken ct)
    {
        switch (user.Role)
        {
            case UserRole.Teacher:
                await UpdateTeacherProfileAsync(user, request.TeacherProfile, ct);
                break;
            case UserRole.Student:
                await UpdateStudentProfileAsync(user, request.StudentGradeId, ct);
                break;
            case UserRole.Admin:
                await UpdateAdminProfileAsync(user, request.AdminProfile, ct);
                break;
        }
    }

    private async Task UpdateStudentProfileAsync(AuthUser user, Guid? studentGradeId, CancellationToken ct)
    {
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

    private async Task UpdateTeacherProfileAsync(AuthUser user, TeacherProfileUpdateRequest? request, CancellationToken ct)
    {
        if (request is null)
        {
            return;
        }

        var profile = await profileRepository.GetTeacherByUserIdAsync(user.Id, ct);
        if (profile is null)
        {
            profile = TeacherProfile.Create(user.Id);
            profile.UpdateDetails(request.Department, request.Designation, request.Qualification, request.PhoneNumber, request.Address, request.DateOfJoining);
            await profileRepository.AddAsync(profile, ct);
            return;
        }

        profile.UpdateDetails(request.Department, request.Designation, request.Qualification, request.PhoneNumber, request.Address, request.DateOfJoining);
        await profileRepository.UpdateAsync(profile, ct);
    }

    private async Task UpdateAdminProfileAsync(AuthUser user, AdminProfileUpdateRequest? request, CancellationToken ct)
    {
        if (request is null)
        {
            return;
        }

        var profile = await profileRepository.GetAdminByUserIdAsync(user.Id, ct);
        if (profile is null)
        {
            profile = AdminProfile.Create(user.Id);
            profile.UpdateDetails(request.Position, request.PhoneNumber);
            await profileRepository.AddAsync(profile, ct);
            return;
        }

        profile.UpdateDetails(request.Position, request.PhoneNumber);
        await profileRepository.UpdateAsync(profile, ct);
    }
}
