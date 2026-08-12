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
    ISectionRepository sectionRepository,
    IPasswordHasher passwordHasher,
    ITransactionService transactionService,
    ICurrentUser currentUser,
    IProfileProvisioningService profileProvisioningService,
    IEmailSender emailSender) : IAdminUserService
{
    public async Task<List<UserListItemDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        return await UserListItemDtoFactory.BuildAsync(users, profileRepository, sectionRepository, ct);
    }

    private async Task<UserListItemDto> BuildDtoAsync(AuthUser user, CancellationToken ct)
    {
        var dtos = await UserListItemDtoFactory.BuildAsync([user], profileRepository, sectionRepository, ct);
        return dtos[0];
    }

    public async Task<UserListItemDto> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(email, ct))
        {
            throw new DuplicateEmailException(email);
        }

        await UserGuards.EnsureStudentSectionValidAsync(sectionRepository, request.Role, request.StudentSectionId, ct);

        var user = AuthUser.CreatePending(request.FullName, email, passwordHasher.Hash(request.Password), request.Role);
        user.Approve();
        await transactionService.ExecuteAsync(async transactionCt =>
        {
            await userRepository.AddAsync(user, transactionCt);
            await profileProvisioningService.CreateProfileAsync(user, request.StudentSectionId, transactionCt);
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

        await UserGuards.EnsureStudentSectionValidAsync(sectionRepository, user.Role, request.StudentSectionId, ct);

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

    public async Task ResetPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        var newPassword = RandomPasswordGenerator.Generate();
        user.SetPassword(passwordHasher.Hash(newPassword), mustChangePassword: true);
        await userRepository.UpdateAsync(user, ct);

        await emailSender.SendAsync(
            user.Email,
            "Your password has been reset",
            $"<p>Hi {user.FullName},</p><p>An administrator reset your password. Your new temporary password is:</p><p><strong>{newPassword}</strong></p><p>You'll be asked to set a new password the next time you sign in.</p>",
            ct);
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
                await UpdateStudentProfileAsync(user, request.StudentSectionId, ct);
                break;
            case UserRole.Admin:
                await UpdateAdminProfileAsync(user, request.AdminProfile, ct);
                break;
        }
    }

    private async Task UpdateStudentProfileAsync(AuthUser user, Guid? studentSectionId, CancellationToken ct)
    {
        var studentProfile = await profileRepository.GetStudentByUserIdAsync(user.Id, ct);
        if (studentProfile is null)
        {
            await profileRepository.AddAsync(StudentProfile.Create(user.Id, studentSectionId!.Value), ct);
        }
        else
        {
            studentProfile.ChangeSection(studentSectionId!.Value);
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
            profile.UpdateDetails(request.TeacherCode, request.Department, request.Designation, request.Qualification, request.PhoneNumber, request.Address, request.DateOfJoining);
            await profileRepository.AddAsync(profile, ct);
            return;
        }

        profile.UpdateDetails(request.TeacherCode, request.Department, request.Designation, request.Qualification, request.PhoneNumber, request.Address, request.DateOfJoining);
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
