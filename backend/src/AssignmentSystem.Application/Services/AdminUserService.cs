using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Pagination;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Profile;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class AdminUserService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    ISectionRepository sectionRepository,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IProfileProvisioningService profileProvisioningService,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork) : IAdminUserService
{
    public async Task<PagedResult<UserListItemDto>> GetAllUsersAsync(
        PageRequest page,
        string? cursor,
        AccountStatus? status,
        UserRole? role,
        CancellationToken ct = default)
    {
        var (afterCreatedAt, afterId) = cursor is null
            ? (afterCreatedAt: (DateTimeOffset?)null, afterId: (Guid?)null)
            : CursorCodec.DecodeTimestamp(cursor);

        var users = await userRepository.GetPageAsync(page.Limit, afterCreatedAt, afterId, status, role, ct);
        var items = await UserListItemDtoFactory.BuildAsync(users.Items, profileRepository, sectionRepository, ct);
        return new PagedResult<UserListItemDto>(items, users.NextCursor, users.HasMore);
    }

    private async Task<UserListItemDto> BuildDtoAsync(AuthUser user, CancellationToken ct)
    {
        var dtos = await UserListItemDtoFactory.BuildAsync([user], profileRepository, sectionRepository, ct);
        return dtos[0];
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        var (studentProfile, teacherProfile, adminProfile) =
            await ProfileDetailDtoFactory.BuildAsync(user, profileRepository, sectionRepository, ct);

        return new UserDetailDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status,
            user.CreatedAt,
            user.IsActive,
            studentProfile,
            teacherProfile,
            adminProfile);
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
        await userRepository.AddAsync(user, ct);
        await profileProvisioningService.CreateProfileAsync(user, request.StudentSectionId, ct);
        await unitOfWork.SaveAsync(ct);

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

        await userRepository.UpdateAsync(user, ct);
        await UpdateProfileAsync(user, request, ct);
        await unitOfWork.SaveAsync(ct);
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

        user.Delete();
        await userRepository.UpdateAsync(user, ct);
        await profileRepository.SoftDeleteForUserAsync(user.Id, ct);
        await unitOfWork.SaveAsync(ct);
    }

    public async Task ResetPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        var newPassword = RandomPasswordGenerator.Generate();
        user.SetPassword(passwordHasher.Hash(newPassword), mustChangePassword: true);
        await userRepository.UpdateAsync(user, ct);
        await unitOfWork.SaveAsync(ct);

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
                await UpdateStudentProfileAsync(user, request.StudentSectionId, request.StudentProfile, ct);
                break;
            case UserRole.Admin:
                await UpdateAdminProfileAsync(user, request.AdminProfile, ct);
                break;
        }
    }

    private async Task UpdateStudentProfileAsync(AuthUser user, Guid? studentSectionId, StudentProfileUpdateRequest? request, CancellationToken ct)
    {
        var studentProfile = await profileRepository.GetStudentByUserIdAsync(user.Id, ct);
        if (studentProfile is null)
        {
            var newProfile = StudentProfile.Create(user.Id, studentSectionId!.Value);
            newProfile.UpdateDetails(
                request?.RollNumber,
                request?.DateOfBirth,
                request?.Gender,
                request?.GuardianName,
                request?.GuardianPhone,
                request?.Address,
                request?.AdmissionDate);
            await profileRepository.AddAsync(newProfile, ct);
        }
        else
        {
            studentProfile.ChangeSection(studentSectionId!.Value);
            studentProfile.UpdateDetails(
                request?.RollNumber,
                request?.DateOfBirth,
                request?.Gender,
                request?.GuardianName,
                request?.GuardianPhone,
                request?.Address,
                request?.AdmissionDate);
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
