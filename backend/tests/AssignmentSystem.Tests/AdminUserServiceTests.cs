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
    private readonly FakeCurrentUser _currentUser = new();
    private readonly AdminUserService _sut;

    public AdminUserServiceTests()
    {
        _sut = new AdminUserService(_users, _profiles, _grades, _hasher, new FakeTransactionService(), _currentUser, new ProfileProvisioningService(_profiles));
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
    public async Task Create_Admin_CreatesAdminProfile()
    {
        var dto = await _sut.CreateUserAsync(new("Admin Two", "admin2@test.com", "Secret@123", UserRole.Admin));

        Assert.Empty(_profiles.TeacherProfiles);
        Assert.Empty(_profiles.StudentProfiles);
        var profile = Assert.Single(_profiles.AdminProfiles);
        Assert.Equal(dto.Id, profile.AuthUserId);
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

        var dto = await _sut.UpdateUserAsync(user.Id, new("New Name", "new@test.com", AccountStatus.Approved, true, grade.Id));

        Assert.Equal("New Name", user.FullName);
        Assert.Equal("new@test.com", user.Email);
        Assert.Equal(UserRole.Student, user.Role);
        Assert.Equal(AccountStatus.Approved, user.Status);
        Assert.True(user.IsActive);
        Assert.Equal(user.Id, dto.Id);
    }

    [Fact]
    public async Task Update_RejectedAlwaysDeactivates()
    {
        var user = AuthUser.CreatePending("Name", "r@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);

        await _sut.UpdateUserAsync(user.Id, new("Name", "r@test.com", AccountStatus.Rejected, true));

        Assert.Equal(AccountStatus.Rejected, user.Status);
        Assert.False(user.IsActive);
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
    public async Task Update_LastUsableAdmin_RejectedThrowsDomainException()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        _users.Users.Add(admin);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Rejected, true)));

        Assert.Equal(AccountStatus.Approved, admin.Status);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task Update_LastUsableAdmin_DeactivatedThrowsDomainException()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        _users.Users.Add(admin);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Approved, false)));

        Assert.Equal(AccountStatus.Approved, admin.Status);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task Update_NonLastAdmin_CanBeRejected()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _users.Users.Add(admin);
        _users.Users.Add(other);

        await _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Rejected, true));

        Assert.Equal(AccountStatus.Rejected, admin.Status);
        Assert.False(admin.IsActive);
    }

    [Fact]
    public async Task Update_NonLastAdmin_CanBeDeactivated()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _users.Users.Add(admin);
        _users.Users.Add(other);

        await _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Approved, false));

        Assert.Equal(AccountStatus.Approved, admin.Status);
        Assert.False(admin.IsActive);
    }

    [Fact]
    public async Task Update_AlreadyUnusableAdmin_UnaffectedByLastAdminGuard()
    {
        var usableAdmin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        usableAdmin.Approve();
        var rejectedAdmin = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        rejectedAdmin.Approve();
        rejectedAdmin.Reject();
        _users.Users.Add(usableAdmin);
        _users.Users.Add(rejectedAdmin);

        await _sut.UpdateUserAsync(rejectedAdmin.Id, new("Admin Two", "admin2@test.com", AccountStatus.Rejected, false));

        Assert.Equal(AccountStatus.Rejected, rejectedAdmin.Status);
        Assert.False(rejectedAdmin.IsActive);
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
    public async Task Update_TeacherWithProfile_CreatesTeacherProfile()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);
        var dateOfJoining = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

        await _sut.UpdateUserAsync(user.Id, new("Teacher", "t@test.com", AccountStatus.Approved, true,
            TeacherProfile: new TeacherProfileUpdateRequest("Science", "Senior Teacher", "MSc", "0170000000", "Dhaka", dateOfJoining)));

        var profile = Assert.Single(_profiles.TeacherProfiles);
        Assert.Equal(user.Id, profile.AuthUserId);
        Assert.Equal("Science", profile.Department);
        Assert.Equal("Senior Teacher", profile.Designation);
        Assert.Equal("MSc", profile.Qualification);
        Assert.Equal("0170000000", profile.PhoneNumber);
        Assert.Equal("Dhaka", profile.Address);
        Assert.Equal(dateOfJoining, profile.DateOfJoining);
    }

    [Fact]
    public async Task Update_TeacherProfileChange_UpdatesExistingProfile()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);
        var profile = TeacherProfile.Create(user.Id);
        profile.UpdateDetails("Old Dept", "Old Title", "Old Qual", "Old Phone", "Old Address", null);
        _profiles.TeacherProfiles.Add(profile);

        await _sut.UpdateUserAsync(user.Id, new("Teacher", "t@test.com", AccountStatus.Approved, true,
            TeacherProfile: new TeacherProfileUpdateRequest("New Dept", "New Title", "New Qual", "New Phone", "New Address", null)));

        Assert.Single(_profiles.TeacherProfiles);
        Assert.Equal("New Dept", profile.Department);
        Assert.Equal("New Title", profile.Designation);
        Assert.Equal("New Qual", profile.Qualification);
        Assert.Equal("New Phone", profile.PhoneNumber);
        Assert.Equal("New Address", profile.Address);
    }

    [Fact]
    public async Task Update_TeacherWithoutProfileInRequest_LeavesProfileUnchanged()
    {
        var user = AuthUser.CreatePending("Teacher", "t@test.com", "hash", UserRole.Teacher);
        user.Approve();
        _users.Users.Add(user);
        var profile = TeacherProfile.Create(user.Id);
        profile.UpdateDetails("Dept", "Title", "Qual", "Phone", "Address", null);
        _profiles.TeacherProfiles.Add(profile);

        await _sut.UpdateUserAsync(user.Id, new("Teacher", "t@test.com", AccountStatus.Approved, true));

        Assert.Single(_profiles.TeacherProfiles);
        Assert.Equal("Dept", profile.Department);
    }

    [Fact]
    public async Task Update_AdminWithProfile_CreatesAdminProfile()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _users.Users.Add(admin);
        _users.Users.Add(other);

        await _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Approved, true,
            AdminProfile: new AdminProfileUpdateRequest("Principal", "0180000000")));

        var profile = Assert.Single(_profiles.AdminProfiles);
        Assert.Equal(admin.Id, profile.AuthUserId);
        Assert.Equal("Principal", profile.Position);
        Assert.Equal("0180000000", profile.PhoneNumber);
    }

    [Fact]
    public async Task Update_AdminProfileChange_UpdatesExistingProfile()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _users.Users.Add(admin);
        _users.Users.Add(other);
        var profile = AdminProfile.Create(admin.Id);
        profile.UpdateDetails("Old Position", "Old Phone");
        _profiles.AdminProfiles.Add(profile);

        await _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Approved, true,
            AdminProfile: new AdminProfileUpdateRequest("New Position", "New Phone")));

        Assert.Single(_profiles.AdminProfiles);
        Assert.Equal("New Position", profile.Position);
        Assert.Equal("New Phone", profile.PhoneNumber);
    }

    [Fact]
    public async Task Update_AdminWithoutProfileInRequest_LeavesProfileUnchanged()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _users.Users.Add(admin);
        _users.Users.Add(other);
        var profile = AdminProfile.Create(admin.Id);
        profile.UpdateDetails("Position", "Phone");
        _profiles.AdminProfiles.Add(profile);

        await _sut.UpdateUserAsync(admin.Id, new("Admin", "admin@test.com", AccountStatus.Approved, true));

        Assert.Single(_profiles.AdminProfiles);
        Assert.Equal("Position", profile.Position);
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
    public async Task Delete_SoftDeletesUserProfile()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var user = AuthUser.CreatePending("Student", "s@test.com", "hash", UserRole.Student);
        user.Approve();
        _users.Users.Add(user);
        var profile = StudentProfile.Create(user.Id, grade.Id);
        _profiles.StudentProfiles.Add(profile);

        await _sut.DeleteUserAsync(user.Id);

        Assert.True(user.IsDeleted);
        Assert.True(profile.IsDeleted);
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
    public async Task Delete_OwnAccount_ThrowsDomainException()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        _users.Users.Add(admin);
        _currentUser.UserId = admin.Id.ToString();

        await Assert.ThrowsAsync<DomainException>(() => _sut.DeleteUserAsync(admin.Id));

        Assert.False(admin.IsDeleted);
    }

    [Fact]
    public async Task Delete_LastAdmin_ThrowsDomainException()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        _users.Users.Add(admin);
        _currentUser.UserId = Guid.NewGuid().ToString();

        await Assert.ThrowsAsync<DomainException>(() => _sut.DeleteUserAsync(admin.Id));

        Assert.False(admin.IsDeleted);
    }

    [Fact]
    public async Task Delete_NonLastAdmin_Deletes()
    {
        var admin = AuthUser.CreatePending("Admin", "admin@test.com", "hash", UserRole.Admin);
        admin.Approve();
        var other = AuthUser.CreatePending("Admin Two", "admin2@test.com", "hash", UserRole.Admin);
        other.Approve();
        _users.Users.Add(admin);
        _users.Users.Add(other);
        _currentUser.UserId = Guid.NewGuid().ToString();

        await _sut.DeleteUserAsync(admin.Id);

        Assert.True(admin.IsDeleted);
        Assert.False(other.IsDeleted);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllUsers()
    {
        _users.Users.Add(AuthUser.CreatePending("One", "one@test.com", "hash", UserRole.Student));
        _users.Users.Add(AuthUser.CreatePending("Two", "two@test.com", "hash", UserRole.Teacher));

        var users = await _sut.GetAllUsersAsync();

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetAllUsers_IncludesIsActiveAndStudentGrade()
    {
        var grade = Grade.Create("Grade 6", "2026");
        _grades.Grades.Add(grade);
        var student = AuthUser.CreatePending("Student One", "student@test.com", "hash", UserRole.Student);
        student.Approve();
        _users.Users.Add(student);
        _profiles.StudentProfiles.Add(StudentProfile.Create(student.Id, grade.Id));

        var teacher = AuthUser.CreatePending("Teacher One", "teacher@test.com", "hash", UserRole.Teacher);
        teacher.Approve();
        teacher.Deactivate();
        _users.Users.Add(teacher);

        var users = await _sut.GetAllUsersAsync();

        var studentDto = users.Single(u => u.Id == student.Id);
        Assert.True(studentDto.IsActive);
        Assert.Equal(grade.Id, studentDto.StudentGradeId);
        Assert.Equal("Grade 6", studentDto.GradeName);

        var teacherDto = users.Single(u => u.Id == teacher.Id);
        Assert.False(teacherDto.IsActive);
        Assert.Null(teacherDto.StudentGradeId);
        Assert.Null(teacherDto.GradeName);
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

        public Task<List<Grade>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
            => Task.FromResult(Grades.Where(g => ids.Contains(g.Id)).ToList());

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

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public string? UserId { get; set; }
    }
}
