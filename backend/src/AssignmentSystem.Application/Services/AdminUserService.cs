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
    ITransactionService transactionService,
    ICurrentUser currentUser,
    IProfileProvisioningService profileProvisioningService,
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
        await transactionService.ExecuteAsync(async transactionCt =>
        {
            await userRepository.AddAsync(user, transactionCt);
            await profileProvisioningService.CreateProfileAsync(user, request.StudentGradeId, transactionCt);
        }, ct);

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

        var willBecomeUnusableAdmin = user.Role == UserRole.Admin
            && user.Status == AccountStatus.Approved
            && user.IsActive
            && (request.Status == AccountStatus.Rejected
                || (request.Status == AccountStatus.Approved && !request.IsActive));

        if (willBecomeUnusableAdmin && await userRepository.CountUsableAdminsAsync(ct) <= 1)
        {
            throw new DomainException("The last admin account cannot be deactivated or rejected.");
        }

        user.UpdateDetails(request.FullName, email);

        if (request.Status == AccountStatus.Approved)
        {
            if (user.Status != AccountStatus.Approved)
            {
                user.Approve();
            }

            if (request.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }
        }
        else if (request.Status == AccountStatus.Rejected)
        {
            user.Reject();
        }

        await transactionService.ExecuteAsync(async transactionCt =>
        {
            await userRepository.UpdateAsync(user, transactionCt);
            await UpdateProfileAsync(user, request, transactionCt);
        }, ct);
        return mapper.Map<UserListItemDto>(user);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");

        if (currentUser.UserId is not null && user.Id.ToString() == currentUser.UserId)
        {
            throw new DomainException("You cannot delete your own account.");
        }

        if (user.Role == UserRole.Admin
            && user.Status == AccountStatus.Approved
            && user.IsActive
            && await userRepository.CountUsableAdminsAsync(ct) <= 1)
        {
            throw new DomainException("The last admin account cannot be deleted.");
        }

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
