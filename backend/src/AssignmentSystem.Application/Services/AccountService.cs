using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Profile;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Services;

public class AccountService(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    ISectionRepository sectionRepository,
    ISystemSettingService systemSettingService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAccountService
{
    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        return await ToDtoAsync(user, ct);
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        user.UpdateDetails(request.FullName, user.Email);
        await userRepository.UpdateAsync(user, ct);

        switch (user.Role)
        {
            case UserRole.Teacher:
                await UpdateTeacherProfileAsync(user, request.TeacherProfile, ct);
                break;
            case UserRole.Student:
                await UpdateStudentProfileAsync(user, request.StudentProfile, ct);
                break;
            case UserRole.Admin:
                await UpdateAdminProfileAsync(user, request.AdminProfile, ct);
                break;
        }

        return await ToDtoAsync(user, ct);
    }

    private async Task UpdateTeacherProfileAsync(AuthUser user, TeacherProfileUpdateRequest? request, CancellationToken ct)
    {
        if (request is null)
        {
            return;
        }

        await systemSettingService.EnsureProfileEditAllowedAsync(UserRole.Teacher, ct);

        var profile = await profileRepository.GetTeacherByUserIdAsync(user.Id, ct);
        if (profile is null)
        {
            profile = TeacherProfile.Create(user.Id);
            profile.UpdateDetails(profile.TeacherCode, request.Department, request.Designation, request.Qualification, request.PhoneNumber, request.Address, profile.DateOfJoining);
            await profileRepository.AddAsync(profile, ct);
            return;
        }

        profile.UpdateDetails(profile.TeacherCode, request.Department, request.Designation, request.Qualification, request.PhoneNumber, request.Address, profile.DateOfJoining);
        await profileRepository.UpdateAsync(profile, ct);
    }

    private async Task UpdateStudentProfileAsync(AuthUser user, StudentProfileUpdateRequest? request, CancellationToken ct)
    {
        if (request is null)
        {
            return;
        }

        await systemSettingService.EnsureProfileEditAllowedAsync(UserRole.Student, ct);

        var profile = await profileRepository.GetStudentByUserIdAsync(user.Id, ct)
            ?? throw new EntityNotFoundException($"Student profile for user {user.Id} was not found.");

        profile.UpdateDetails(
            profile.RollNumber,
            request.DateOfBirth,
            request.Gender,
            request.GuardianName,
            request.GuardianPhone,
            request.Address,
            profile.AdmissionDate);
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

    /// <summary>
    /// Reissues tokens after the change so the current session survives, while the prior refresh
    /// token (and therefore any other session) is revoked.
    /// </summary>
    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await GetUserAsync(userId, ct);
        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidCurrentPasswordException();
        }

        user.SetPassword(passwordHasher.Hash(request.NewPassword));

        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = tokenService.CreateRefreshToken();
        user.SetRefreshToken(refreshToken, tokenService.RefreshTokenExpiresAt, tokenService.RefreshTokenGraceExpiresAt);
        await userRepository.UpdateAsync(user, ct);

        return new AuthResponse(
            accessToken,
            refreshToken,
            tokenService.AccessTokenExpiresAt,
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.Status,
            user.MustChangePassword);
    }

    private async Task<AuthUser> GetUserAsync(Guid userId, CancellationToken ct)
    {
        return await userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException($"User with id {userId} was not found.");
    }

    private async Task<ProfileDto> ToDtoAsync(AuthUser user, CancellationToken ct)
    {
        var (studentProfile, teacherProfile, adminProfile) =
            await ProfileDetailDtoFactory.BuildAsync(user, profileRepository, sectionRepository, ct);

        var canEditProfile = user.Role == UserRole.Admin
            || await CanEditProfileAsync(user.Role, ct);

        return new ProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.MustChangePassword,
            canEditProfile,
            studentProfile,
            teacherProfile,
            adminProfile);
    }

    private async Task<bool> CanEditProfileAsync(UserRole role, CancellationToken ct)
    {
        var policy = await systemSettingService.GetProfileEditPolicyAsync(ct);
        return role switch
        {
            UserRole.Teacher => policy.TeacherProfileSelfEditEnabled,
            UserRole.Student => policy.StudentProfileSelfEditEnabled,
            _ => false
        };
    }
}