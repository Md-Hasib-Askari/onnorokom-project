using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class AuthServiceTests
{
    private readonly FakeUserRepository _repo = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTokenService _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_repo, _hasher, _tokens);
    }

    [Fact]
    public async Task Register_CreatesPendingUserWithHashedPassword()
    {
        var request = new Application.DTOs.Auth.RegisterRequest("Student One", "student1@test.com", "secret123", UserRole.Student);

        var user = await _sut.RegisterAsync(request);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("student1@test.com", user.Email);
        Assert.Equal("HASH:secret123", user.PasswordHash);
        Assert.Equal(AccountStatus.Pending, user.Status);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowerCase()
    {
        var request = new Application.DTOs.Auth.RegisterRequest("Student One", "  Student1@Test.com ", "secret123", UserRole.Student);

        var user = await _sut.RegisterAsync(request);

        Assert.Equal("student1@test.com", user.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        _repo.Users.Add(CreateUser(email: "existing@test.com"));
        var request = new Application.DTOs.Auth.RegisterRequest("Student Two", "existing@test.com", "secret123", UserRole.Student);

        await Assert.ThrowsAsync<DuplicateEmailException>(() => _sut.RegisterAsync(request));
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
    public async Task Login_InactiveAccount_ThrowsAccountInactiveException()
    {
        var user = CreateUser(email: "inactive@test.com", status: AccountStatus.Approved, password: "correct");
        user.Deactivate();
        _repo.Users.Add(user);

        await Assert.ThrowsAsync<AccountInactiveException>(() => _sut.LoginAsync(new("inactive@test.com", "correct")));
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
    public async Task Approve_UnknownUser_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.ApproveAsync(Guid.NewGuid(), true));
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
        public List<AuthUser> Users { get; } = new();

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

        public Task AddAsync(AuthUser user, CancellationToken ct = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AuthUser user, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"HASH:{password}";

        public bool Verify(string password, string hashedPassword)
            => $"HASH:{password}" == hashedPassword;
    }

    private sealed class FakeTokenService : ITokenService
    {
        public DateTimeOffset AccessTokenExpiresAt { get; } = DateTimeOffset.UtcNow.AddMinutes(15);
        public DateTimeOffset RefreshTokenExpiresAt { get; } = DateTimeOffset.UtcNow.AddDays(7);

        public string CreateAccessToken(AuthUser user) => "access-token";

        public string CreateRefreshToken() => "refresh-token";
    }
}
