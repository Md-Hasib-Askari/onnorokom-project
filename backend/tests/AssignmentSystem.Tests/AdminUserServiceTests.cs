using AssignmentSystem.Application.Common.Exceptions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.DTOs.Admin;
using AssignmentSystem.Application.Services;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Tests;

public class AdminUserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProfileRepository _profiles = new();
    private readonly FakeGradeRepository _grades = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly AdminUserService _sut;

    public AdminUserServiceTests()
    {
        _sut = new AdminUserService(_users, _profiles, _grades, _hasher, TestMappers.CreateMapper());
    }

    [Fact]
    public async Task Create_StudentWithGrade_IsApprovedAndProfileCreated()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);

        var dto = await _sut.CreateUserAsync(new("Student One", "student@test.com", "Secret@123", UserRole.Student, grade.Id));

        var user = _users.Users.Single();
        Assert.Equal(AccountStatus.Approved, user.Status);
        Assert.Equal("HASH:Secret@123", user.PasswordHash);
        Assert.Single(_profiles.StudentProfiles);
        Assert.Equal(user.Id, _profiles.StudentProfiles[0].AuthUserId);
        Assert.Equal(grade.Id, _profiles.StudentProfiles[0].GradeId);
        Assert.Equal(user.Id, dto.Id);
    }

    [Fact]
    public async Task Create_StudentWithoutGrade_ThrowsDomainExceptionAndNoProfile()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateUserAsync(new("Student One", "student@test.com", "Secret@123", UserRole.Student)));

        Assert.Empty(_users.Users);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Create_Teacher_CreatesTeacherProfile()
    {
        await _sut.CreateUserAsync(new("Teacher One", "teacher@test.com", "Secret@123", UserRole.Teacher));

        var user = _users.Users.Single();
        Assert.Single(_profiles.TeacherProfiles);
        Assert.Equal(user.Id, _profiles.TeacherProfiles[0].AuthUserId);
    }

    [Fact]
    public async Task Create_Admin_NoProfileCreated()
    {
        await _sut.CreateUserAsync(new("Admin Two", "admin2@test.com", "Secret@123", UserRole.Admin));

        Assert.Empty(_profiles.TeacherProfiles);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Create_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        var user = AuthUser.CreatePending("Existing", "dup@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _sut.CreateUserAsync(new("New", "DUP@test.com", "Secret@123", UserRole.Student)));
    }

    [Fact]
    public async Task Create_StudentWithUnknownGrade_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.CreateUserAsync(new("Student One", "student@test.com", "Secret@123", UserRole.Student, Guid.NewGuid())));

        Assert.Empty(_users.Users);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Update_ChangesDetailsStatusAndActivity()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var user = AuthUser.CreatePending("Old Name", "old@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        var dto = await _sut.UpdateUserAsync(user.Id, new("New Name", "new@test.com", AccountStatus.Approved, false, grade.Id));

        Assert.Equal("New Name", user.FullName);
        Assert.Equal("new@test.com", user.Email);
        Assert.Equal(UserRole.Student, user.Role);
        Assert.Equal(AccountStatus.Approved, user.Status);
        Assert.False(user.IsActive);
        Assert.Equal(user.Id, dto.Id);
    }

    [Fact]
    public async Task Update_RejectedStatus_SetsRejected()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var user = AuthUser.CreatePending("Name", "a@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await _sut.UpdateUserAsync(user.Id, new("Name", "a@test.com", AccountStatus.Rejected, true, grade.Id));

        Assert.Equal(AccountStatus.Rejected, user.Status);
    }

    [Fact]
    public async Task Update_StudentWithoutGrade_ThrowsDomainExceptionAndKeepsUserUnchanged()
    {
        var user = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateUserAsync(user.Id, new("New Name", "s@test.com", AccountStatus.Approved, true)));

        Assert.Equal("Student", user.FullName);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Update_PendingStatus_ThrowsDomainExceptionAndKeepsUserUnchanged()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateUserAsync(user.Id, new("New Name", "t@test.com", AccountStatus.Pending, true)));

        Assert.Equal("Teacher", user.FullName);
        Assert.Equal(AccountStatus.Approved, user.Status);
    }

    [Fact]
    public async Task Update_StudentWithGrade_CreatesStudentProfile()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var user = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await _sut.UpdateUserAsync(user.Id, new("Student", "s@test.com", AccountStatus.Approved, true, grade.Id));

        var profile = _profiles.StudentProfiles.Single();
        Assert.Equal(user.Id, profile.AuthUserId);
        Assert.Equal(grade.Id, profile.GradeId);
    }

    [Fact]
    public async Task Update_StudentGradeChange_UpdatesExistingProfile()
    {
        var gradeA = Grade.Create("Grade 6", "2026");
        var gradeB = Grade.Create("Grade 7", "2026");
        _grades.Grades.Add(gradeA);
        _grades.Grades.Add(gradeB);
        var user = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);
        var profile = StudentProfile.Create(user.Id, gradeA.Id);
        _profiles.StudentProfiles.Add(profile);

        await _sut.UpdateUserAsync(user.Id, new("Student", "s@test.com", AccountStatus.Approved, true, gradeB.Id));

        Assert.Single(_profiles.StudentProfiles);
        Assert.Equal(gradeB.Id, profile.GradeId);
    }

    [Fact]
    public async Task Update_StudentWithUnknownGrade_ThrowsAndKeepsUserUnchanged()
    {
        var user = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateUserAsync(user.Id, new("New Name", "s@test.com", AccountStatus.Approved, true, Guid.NewGuid())));

        Assert.Equal("Student", user.FullName);
        Assert.Empty(_profiles.StudentProfiles);
    }

    [Fact]
    public async Task Update_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        var other = AuthUser.CreatePending("Other", "taken@test.com", "hash", UserRole.Student);
        other.Approve();
        _users.Users.Add(other);
        var user = AuthUser.CreatePending("Mine", "mine@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _sut.UpdateUserAsync(user.Id, new("Mine", "taken@test.com", AccountStatus.Approved, true)));
    }

    [Fact]
    public async Task Update_UnknownUser_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.UpdateUserAsync(Guid.NewGuid(), new("Name", "a@test.com", AccountStatus.Approved, true)));
    }

    [Fact]
    public async Task Delete_MarksUserAsDeleted()
    {
        var user = AuthUser.CreatePending("Name", "a@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);

        await _sut.DeleteUserAsync(user.Id);

        Assert.True(user.IsDeleted);
    }

    [Fact]
    public async Task Delete_TeacherWithAssignedSubjects_ThrowsEntityInUseException()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);
        _users.AssignedSubjectUserIds.Add(user.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteUserAsync(user.Id));

        Assert.False(user.IsDeleted);
    }

    [Fact]
    public async Task Delete_TeacherWithAssignments_ThrowsEntityInUseException()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);
        _users.AssignmentUserIds.Add(user.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteUserAsync(user.Id));

        Assert.False(user.IsDeleted);
    }

    [Fact]
    public async Task Delete_StudentWithSubmissions_ThrowsEntityInUseException()
    {
        var user = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);
        _users.SubmissionUserIds.Add(user.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteUserAsync(user.Id));

        Assert.False(user.IsDeleted);
    }

    [Fact]
    public async Task Delete_TeacherWithGradedSubmissions_ThrowsEntityInUseException()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);
        _users.GradedSubmissionUserIds.Add(user.Id);

        await Assert.ThrowsAsync<EntityInUseException>(() => _sut.DeleteUserAsync(user.Id));

        Assert.False(user.IsDeleted);
    }

    [Fact]
    public async Task Delete_UnknownUser_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _sut.DeleteUserAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllUsers()
    {
        _users.Users.Add(AuthUser.CreatePending("One", "one@test.com", "hash", UserRole.Student));
        _users.Users.Add(AuthUser.CreatePending("Two", "two@test.com", "hash", UserRole.Teacher));

        var users = await _sut.GetAllUsersAsync();

        Assert.Equal(2, users.Count);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<AuthUser> Users { get; } = new();
        public List<Guid> AssignedSubjectUserIds { get; } = new();
        public List<Guid> AssignmentUserIds { get; } = new();
        public List<Guid> SubmissionUserIds { get; } = new();
        public List<Guid> GradedSubmissionUserIds { get; } = new();

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
            => Task.FromResult(AssignedSubjectUserIds.Contains(userId));

        public Task<bool> HasAssignmentsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(AssignmentUserIds.Contains(userId));

        public Task<bool> HasSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(SubmissionUserIds.Contains(userId));

        public Task<bool> HasGradedSubmissionsAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(GradedSubmissionUserIds.Contains(userId));

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

        public Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default)
            => Task.FromResult(StudentProfiles.FirstOrDefault(p => p.AuthUserId == authUserId));

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

        public Task UpdateAsync(StudentProfile profile, CancellationToken ct = default)
            => Task.CompletedTask;
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
}
