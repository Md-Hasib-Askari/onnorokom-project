using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.DTOs.Settings;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class AuthServiceTests
{
    private readonly FakeUserRepository _repo = new();
    private readonly FakeProfileRepository _profiles = new();
    private readonly FakeSectionRepository _sections = new();
    private readonly FakePasswordResetCodeRepository _resetCodes = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTokenService _tokens = new();
    private readonly FakeSystemSettingService _settings = new();
    private readonly FakeEmailSender _emailSender = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _repo, _sections, _profiles, _resetCodes, _hasher, _tokens, new FakeTransactionService(),
            new ProfileProvisioningService(_profiles), _settings, _emailSender,
            Options.Create(new PasswordResetSettings()));
    }

    private Section AddSection(string name = "Section A")
    {
        var grade = Grade.Create("Grade 6", "2026");
        var section = Section.Create(name, grade.Id);
        _sections.Sections.Add(section);
        return section;
    }

    [Fact]
    public async Task Register_CreatesPendingUserWithHashedPassword()
    {
        var request = new RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student);

        var user = await _sut.RegisterAsync(request);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("student1@test.com", user.Email);
        Assert.Equal("HASH:secret123", user.PasswordHash);
        Assert.Equal(AccountStatus.Pending, user.Status);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Register_Student_DefersProfileUntilAdminAssignsSection()
    {
        var request = new RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student);

        await _sut.RegisterAsync(request);

        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowerCase()
    {
        var request = new RegisterRequest("Student One", "  Student1@Test.com ", "secret123", UserRole.Student);

        var user = await _sut.RegisterAsync(request);

        Assert.Equal("student1@test.com", user.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        _repo.Users.Add(CreateUser(email: "existing@test.com"));
        var request = new RegisterRequest("Student Two", "existing@test.com", "secret123", UserRole.Student);

        await Assert.ThrowsAsync<DuplicateEmailException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task Register_StudentWhenSelfRegistrationClosed_ThrowsAndCreatesNothing()
    {
        _settings.StudentSelfRegistrationEnabled = false;
        var request = new RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student);

        await Assert.ThrowsAsync<RegistrationDisabledException>(() => _sut.RegisterAsync(request));

        Assert.Empty(_repo.Users);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Register_TeacherWhenSelfRegistrationClosed_ThrowsAndCreatesNothing()
    {
        _settings.TeacherSelfRegistrationEnabled = false;
        var request = new RegisterRequest("Teacher One", "teacher1@test.com", "secret123", UserRole.Teacher);

        await Assert.ThrowsAsync<RegistrationDisabledException>(() => _sut.RegisterAsync(request));

        Assert.Empty(_repo.Users);
        Assert.Empty(_profiles.TeacherProfiles);
    }

    /// <summary>
    /// The policy gate runs before the email lookup, so a closed role cannot be used to probe which
    /// addresses already exist.
    /// </summary>
    [Fact]
    public async Task Register_ClosedRoleWithTakenEmail_ReportsClosureNotDuplicate()
    {
        _settings.TeacherSelfRegistrationEnabled = false;
        _repo.Users.Add(CreateUser(email: "existing@test.com"));
        var request = new RegisterRequest("Teacher One", "existing@test.com", "secret123", UserRole.Teacher);

        await Assert.ThrowsAsync<RegistrationDisabledException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task Register_Teacher_CreatesTeacherProfile()
    {
        var request = new RegisterRequest("Teacher One", "teacher1@test.com", "secret123", UserRole.Teacher);

        var user = await _sut.RegisterAsync(request);

        Assert.Equal(UserRole.Teacher, user.Role);
        Assert.Single(_profiles.TeacherProfiles);
        Assert.Equal(user.Id, _profiles.TeacherProfiles[0].AuthUserId);
    }

    /// <summary>
    /// No setting can open the Admin role to public sign-up, so the service refuses it even with
    /// both toggles on.
    /// </summary>
    [Fact]
    public async Task Register_Admin_ThrowsRegistrationDisabledException()
    {
        var request = new RegisterRequest("Admin One", "admin1@test.com", "secret123", UserRole.Admin);

        await Assert.ThrowsAsync<RegistrationDisabledException>(() => _sut.RegisterAsync(request));

        Assert.Empty(_repo.Users);
        Assert.Empty(_profiles.AdminProfiles);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokensAndUserInfo()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved, password: "correct");
        _repo.Users.Add(user);

        var response = await _sut.LoginAsync(new("approved@test.com", "correct"));

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(UserRole.Student, response.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved, password: "correct");
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(new("approved@test.com", "wrong")));
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsInvalidCredentialsException()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(new("nobody@test.com", "whatever")));
    }

    [Fact]
    public async Task Login_PendingAccount_ThrowsAccountPendingException()
    {
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending, password: "correct");
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<AccountPendingException>(() => _sut.LoginAsync(new("pending@test.com", "correct")));
    }

    [Fact]
    public async Task Login_RejectedAccount_ThrowsAccountRejectedException()
    {
        var user = CreateUser(email: "rejected@test.com", status: AccountStatus.Rejected, password: "correct");
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<AccountRejectedException>(() => _sut.LoginAsync(new("rejected@test.com", "correct")));
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesTokens()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved);
        user.SetRefreshToken("valid-refresh", DateTimeOffset.UtcNow.AddMinutes(5), _tokens.RefreshTokenGraceExpiresAt);
        _repo.Users.Add(user);

        var response = await _sut.RefreshAsync("valid-refresh");

        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal("refresh-token", user.RefreshToken);
    }

    [Fact]
    public async Task Refresh_RaceOnJustRotatedToken_ReturnsCurrentTokensInsteadOfThrowing()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved);
        user.SetRefreshToken("original-refresh", DateTimeOffset.UtcNow.AddMinutes(5), _tokens.RefreshTokenGraceExpiresAt);
        _repo.Users.Add(user);

        // Winner of the race rotates the token first.
        var winnerResponse = await _sut.RefreshAsync("original-refresh");
        Assert.Equal("refresh-token", winnerResponse.RefreshToken);

        // Loser presents the now-superseded token within the grace window instead of failing.
        var loserResponse = await _sut.RefreshAsync("original-refresh");

        Assert.Equal(user.RefreshToken, loserResponse.RefreshToken);
        Assert.Equal("refresh-token", loserResponse.RefreshToken);
    }

    [Fact]
    public async Task Refresh_TokenOutsideGraceWindow_ThrowsInvalidRefreshTokenException()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved);
        user.SetRefreshToken("original-refresh", DateTimeOffset.UtcNow.AddMinutes(5), DateTimeOffset.UtcNow.AddMinutes(-1));
        user.SetRefreshToken("rotated-refresh", DateTimeOffset.UtcNow.AddMinutes(5), DateTimeOffset.UtcNow.AddMinutes(-1));
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.RefreshAsync("original-refresh"));
    }

    [Fact]
    public async Task Refresh_UnknownToken_ThrowsInvalidRefreshTokenException()
    {
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.RefreshAsync("unknown-token"));
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ThrowsInvalidRefreshTokenException()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved);
        user.SetRefreshToken("expired-refresh", DateTimeOffset.UtcNow.AddMinutes(-1), _tokens.RefreshTokenGraceExpiresAt);
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.RefreshAsync("expired-refresh"));
    }

    [Fact]
    public async Task Logout_ValidToken_RevokesRefreshToken()
    {
        var user = CreateUser(email: "approved@test.com", status: AccountStatus.Approved);
        user.SetRefreshToken("valid-refresh", DateTimeOffset.UtcNow.AddMinutes(5), _tokens.RefreshTokenGraceExpiresAt);
        _repo.Users.Add(user);

        await _sut.LogoutAsync("valid-refresh");

        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task Logout_UnknownToken_DoesNotThrow()
    {
        await _sut.LogoutAsync("unknown-token");
    }

    [Fact]
    public async Task Approve_ApproveSetsStatusApproved()
    {
        var section = AddSection();
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);

        var result = await _sut.ApproveAsync(user.Id, true, section.Id);

        Assert.Equal(AccountStatus.Approved, result.Status);
    }

    [Fact]
    public async Task Approve_Student_EnrolsIntoTheSectionTheAdminChose()
    {
        var section = AddSection();
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);

        await _sut.ApproveAsync(user.Id, true, section.Id);

        var profile = Assert.Single(_profiles.StudentProfiles);
        Assert.Equal(user.Id, profile.AuthUserId);
        Assert.Equal(section.Id, profile.SectionId);
    }

    [Fact]
    public async Task Approve_StudentWithoutSection_ThrowsDomainExceptionAndLeavesUserPending()
    {
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<DomainException>(() => _sut.ApproveAsync(user.Id, true));

        Assert.Equal(AccountStatus.Pending, user.Status);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Approve_StudentWithUnknownSection_ThrowsEntityNotFoundException()
    {
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.ApproveAsync(user.Id, true, Guid.NewGuid()));

        Assert.Equal(AccountStatus.Pending, user.Status);
        Assert.Empty(_profiles.StudentProfiles);
    }

    /// <summary>
    /// An admin-created student already has a section, so approval must not need one and must not
    /// add a second profile.
    /// </summary>
    [Fact]
    public async Task Approve_StudentWithExistingProfile_NeedsNoSection()
    {
        var section = AddSection();
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);
        _profiles.StudentProfiles.Add(StudentProfile.Create(user.Id, section.Id));

        var result = await _sut.ApproveAsync(user.Id, true);

        Assert.Equal(AccountStatus.Approved, result.Status);
        Assert.Single(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Approve_Teacher_NeedsNoSection()
    {
        var user = AuthUser.CreatePending("Teacher", "teacher@test.com", "hash", UserRole.Teacher);
        _repo.Users.Add(user);

        var result = await _sut.ApproveAsync(user.Id, true);

        Assert.Equal(AccountStatus.Approved, result.Status);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Approve_RejectSetsStatusRejected()
    {
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);

        var result = await _sut.ApproveAsync(user.Id, false);

        Assert.Equal(AccountStatus.Rejected, result.Status);
    }

    [Fact]
    public async Task Approve_RejectedUser_SetsStatusApproved()
    {
        var section = AddSection();
        var user = CreateUser(email: "rejected@test.com", status: AccountStatus.Rejected);
        _repo.Users.Add(user);

        var result = await _sut.ApproveAsync(user.Id, true, section.Id);

        Assert.Equal(AccountStatus.Approved, result.Status);
    }

    [Fact]
    public async Task Approve_AlreadyApprovedAndDeactivated_ThrowsDomainExceptionAndStaysDeactivated()
    {
        var user = AuthUser.CreatePending("Name", "u@test.com", "hash", UserRole.Student);
        user.Approve();
        user.Deactivate();
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<DomainException>(() => _sut.ApproveAsync(user.Id, true));

        Assert.Equal(AccountStatus.Approved, user.Status);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task Approve_UnknownUser_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.ApproveAsync(Guid.NewGuid(), true));
    }

    [Fact]
    public async Task Approve_RejectLastUsableAdmin_ThrowsDomainException()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        _repo.Users.Add(admin);

        await Assert.ThrowsAsync<DomainException>(() => _sut.ApproveAsync(admin.Id, false));

        Assert.Equal(AccountStatus.Approved, admin.Status);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task Approve_RejectNonLastAdmin_Succeeds()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _repo.Users.Add(admin);
        _repo.Users.Add(other);

        var result = await _sut.ApproveAsync(admin.Id, false);

        Assert.Equal(AccountStatus.Rejected, result.Status);
        Assert.False(result.IsActive);
    }

    private static AuthUser CreateUser(string email, AccountStatus status = AccountStatus.Pending, string password = "secret123")
    {
        var user = AuthUser.CreatePending("Test User", email, $"HASH:{password}", UserRole.Student);

        if (status == AccountStatus.Approved)
        {
            user.Approve();
        }
        else if (status == AccountStatus.Rejected)
        {
            user.Reject();
        }

        return user;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<AuthUser> Users { get; } = [];

        public Task<AuthUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

        public Task<AuthUser?> GetByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

        public Task<AuthUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
            => Task.FromResult(Users.FirstOrDefault(u =>
                u.RefreshToken == refreshToken || u.PreviousRefreshToken == refreshToken));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(Users.Any(u => u.Email == email));

        public Task<List<AuthUser>> GetByStatusAsync(AccountStatus status, CancellationToken ct = default)
            => Task.FromResult(Users.Where(u => u.Status == status).ToList());

        public Task<List<AuthUser>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Users.ToList());

        public Task<bool> HasAssignedSubjectsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<int> CountUsableAdminsAsync(CancellationToken ct = default)
            => Task.FromResult(Users.Count(u => u.Role == UserRole.Admin && u.Status == AccountStatus.Approved && u.IsActive));

        public Task AddAsync(AuthUser user, CancellationToken ct = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AuthUser user, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        public List<TeacherProfile> TeacherProfiles { get; } = new();
        public List<StudentProfile> StudentProfiles { get; } = new();
        public List<AdminProfile> AdminProfiles { get; } = new();

        public Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(StudentProfiles.FirstOrDefault(p => p.AuthUserId == authUserId));

        public Task<List<StudentProfile>> GetStudentsByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
            => Task.FromResult(StudentProfiles.Where(p => authUserIds.Contains(p.AuthUserId)).ToList());

        public Task<TeacherProfile?> GetTeacherByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(TeacherProfiles.FirstOrDefault(p => p.AuthUserId == authUserId));

        public Task<List<TeacherProfile>> GetTeachersByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
            => Task.FromResult(TeacherProfiles.Where(p => authUserIds.Contains(p.AuthUserId)).ToList());

        public Task<AdminProfile?> GetAdminByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(AdminProfiles.FirstOrDefault(p => p.AuthUserId == authUserId));

        public Task AddAsync(TeacherProfile profile, CancellationToken ct = default)
        {
            TeacherProfiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task AddAsync(StudentProfile profile, CancellationToken ct = default)
        {
            StudentProfiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task AddAsync(AdminProfile profile, CancellationToken ct = default)
        {
            AdminProfiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(StudentProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(TeacherProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task UpdateAsync(AdminProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task SoftDeleteForUserAsync(Guid authUserId, CancellationToken ct = default)
        {
            TeacherProfiles.FirstOrDefault(p => p.AuthUserId == authUserId)?.Delete();
            StudentProfiles.FirstOrDefault(p => p.AuthUserId == authUserId)?.Delete();
            AdminProfiles.FirstOrDefault(p => p.AuthUserId == authUserId)?.Delete();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSectionRepository : ISectionRepository
    {
        public List<Section> Sections { get; } = new();

        public Task<List<Section>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Sections.ToList());

        public Task<Section?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Sections.FirstOrDefault(s => s.Id == id));

        public Task<List<Section>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => Task.FromResult(Sections.Where(s => ids.Contains(s.Id)).ToList());

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Sections.Any(s => s.Id == id));

        public Task<bool> ExistsByNameAsync(string name, Guid gradeId, CancellationToken ct = default)
            => Task.FromResult(Sections.Any(s => s.Name == name && s.GradeId == gradeId));

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Section section, CancellationToken ct = default)
        {
            Sections.Add(section);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Section section, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"HASH:{password}";

        public bool Verify(string password, string hashedPassword)
            => $"HASH:{password}" == hashedPassword;
    }

    private sealed class FakeTransactionService : ITransactionService
    {
        public Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
            => work(ct);
    }

    /// <summary>
    /// Both roles start open so the tests that are not about the policy do not have to enable it.
    /// The gate itself is exercised by flipping a flag before calling the SUT.
    /// </summary>
    private sealed class FakeSystemSettingService : ISystemSettingService
    {
        public bool TeacherSelfRegistrationEnabled { get; set; } = true;
        public bool StudentSelfRegistrationEnabled { get; set; } = true;

        public Task<RegistrationPolicyDto> GetRegistrationPolicyAsync(CancellationToken ct = default)
            => Task.FromResult(new RegistrationPolicyDto(
                TeacherSelfRegistrationEnabled, StudentSelfRegistrationEnabled));

        public Task<RegistrationPolicyDto> UpdateRegistrationPolicyAsync(
            RegistrationPolicyUpdateRequest request, CancellationToken ct = default)
        {
            TeacherSelfRegistrationEnabled = request.TeacherSelfRegistrationEnabled;
            StudentSelfRegistrationEnabled = request.StudentSelfRegistrationEnabled;
            return GetRegistrationPolicyAsync(ct);
        }

        public Task EnsureSelfRegistrationAllowedAsync(UserRole role, CancellationToken ct = default)
        {
            var allowed = role switch
            {
                UserRole.Teacher => TeacherSelfRegistrationEnabled,
                UserRole.Student => StudentSelfRegistrationEnabled,
                _ => false
            };

            return allowed ? Task.CompletedTask : throw new RegistrationDisabledException(role);
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        private int _refreshTokensIssued;

        public DateTimeOffset AccessTokenExpiresAt { get; } = DateTimeOffset.UtcNow.AddMinutes(15);
        public DateTimeOffset RefreshTokenExpiresAt { get; } = DateTimeOffset.UtcNow.AddDays(7);
        public DateTimeOffset RefreshTokenGraceExpiresAt { get; } = DateTimeOffset.UtcNow.AddSeconds(30);

        public string CreateAccessToken(AuthUser user) => "access-token";

        public string CreateRefreshToken()
        {
            _refreshTokensIssued++;
            return _refreshTokensIssued == 1 ? "refresh-token" : $"refresh-token-{_refreshTokensIssued}";
        }
    }

    private sealed class FakePasswordResetCodeRepository : IPasswordResetCodeRepository
    {
        public List<PasswordResetCode> Codes { get; } = new();

        public Task<PasswordResetCode?> GetLatestForUserAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(Codes.Where(c => c.AuthUserId == authUserId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefault());

        public Task AddAsync(PasswordResetCode code, CancellationToken ct = default)
        {
            Codes.Add(code);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(PasswordResetCode code, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public List<(string ToEmail, string Subject, string HtmlBody)> SentEmails { get; } = new();

        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            SentEmails.Add((toEmail, subject, htmlBody));
            return Task.CompletedTask;
        }
    }
}
