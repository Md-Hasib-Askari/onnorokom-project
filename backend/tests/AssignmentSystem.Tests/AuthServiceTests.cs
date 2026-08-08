using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class AuthServiceTests
{
    private readonly FakeUserRepository _repo = new();
    private readonly FakeProfileRepository _profiles = new();
    private readonly FakeGradeRepository _grades = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTokenService _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_repo, _grades, _hasher, _tokens, new FakeTransactionService(), new ProfileProvisioningService(_profiles), TestMappers.CreateMapper());
    }

    [Fact]
    public async Task Register_CreatesPendingUserWithHashedPassword()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var request = new RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student, grade.Id);

        var user = await _sut.RegisterAsync(request);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("student1@test.com", user.Email);
        Assert.Equal("HASH:secret123", user.PasswordHash);
        Assert.Equal(AccountStatus.Pending, user.Status);
        Assert.True(user.IsActive);
        Assert.Single(_profiles.StudentProfiles);
        Assert.Equal(grade.Id, _profiles.StudentProfiles[0].GradeId);
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowerCase()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var request = new RegisterRequest("Student One", "  Student1@Test.com ", "secret123", UserRole.Student, grade.Id);

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
    public async Task Register_StudentWithoutGrade_ThrowsDomainExceptionAndCreatesNothing()
    {
        var request = new RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student);

        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(request));

        Assert.Empty(_repo.Users);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Register_StudentWithUnknownGrade_ThrowsEntityNotFoundException()
    {
        var request = new RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student, Guid.NewGuid());

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.RegisterAsync(request));

        Assert.Empty(_repo.Users);
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

    [Fact]
    public async Task Register_Admin_CreatesAdminProfile()
    {
        var request = new RegisterRequest("Admin One", "admin1@test.com", "secret123", UserRole.Admin);

        var user = await _sut.RegisterAsync(request);

        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Single(_profiles.AdminProfiles);
        Assert.Equal(user.Id, _profiles.AdminProfiles[0].AuthUserId);
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
        user.SetRefreshToken("valid-refresh", DateTimeOffset.UtcNow.AddMinutes(5));
        _repo.Users.Add(user);

        var response = await _sut.RefreshAsync("valid-refresh");

        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal("refresh-token", user.RefreshToken);
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
        user.SetRefreshToken("expired-refresh", DateTimeOffset.UtcNow.AddMinutes(-1));
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.RefreshAsync("expired-refresh"));
    }

    [Fact]
    public async Task Approve_ApproveSetsStatusApproved()
    {
        var user = CreateUser(email: "pending@test.com", status: AccountStatus.Pending);
        _repo.Users.Add(user);

        var result = await _sut.ApproveAsync(user.Id, true);

        Assert.Equal(AccountStatus.Approved, result.Status);
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

    [Fact]
    public async Task GetPendingUsers_ReturnsOnlyPending()
    {
        _repo.Users.Add(CreateUser(email: "pending1@test.com", status: AccountStatus.Pending));
        _repo.Users.Add(CreateUser(email: "approved@test.com", status: AccountStatus.Approved));
        _repo.Users.Add(CreateUser(email: "pending2@test.com", status: AccountStatus.Pending));

        var pending = await _sut.GetPendingUsersAsync();

        Assert.Equal(2, pending.Count);
        Assert.All(pending, u => Assert.Equal(AccountStatus.Pending, u.Status));
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
            => Task.FromResult(Users.FirstOrDefault(u => u.RefreshToken == refreshToken));

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

        public Task<TeacherProfile?> GetTeacherByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(TeacherProfiles.FirstOrDefault(p => p.AuthUserId == authUserId));

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

    private sealed class FakeGradeRepository : IGradeRepository
    {
        public List<Grade> Grades { get; } = new();

        public Task<List<Grade>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult(Grades.ToList());

        public Task<Grade?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Grades.FirstOrDefault(g => g.Id == id));

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Grades.Any(g => g.Id == id));

        public Task<bool> ExistsAsync(string name, string academicYear, CancellationToken ct = default)
            => Task.FromResult(Grades.Any(g => g.Name == name && g.AcademicYear == academicYear));

        public Task<bool> HasSubjectsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task<bool> HasStudentsAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(false);

        public Task AddAsync(Grade grade, CancellationToken ct = default)
        {
            Grades.Add(grade);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Grade grade, CancellationToken ct = default)
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

    private sealed class FakeTokenService : ITokenService
    {
        public DateTimeOffset AccessTokenExpiresAt { get; } = DateTimeOffset.UtcNow.AddMinutes(15);
        public DateTimeOffset RefreshTokenExpiresAt { get; } = DateTimeOffset.UtcNow.AddDays(7);

        public string CreateAccessToken(AuthUser user) => "access-token";

        public string CreateRefreshToken() => "refresh-token";
    }
}
