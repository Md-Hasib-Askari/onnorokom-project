using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class ProfileRepository(AppDbContext dbContext) : IProfileRepository
{
    public async Task<StudentProfile?> GetStudentByUserIdAsync(Guid authUserId, CancellationToken ct = default)
    {
        return await dbContext.StudentProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId, ct);
    }

    public async Task<List<StudentProfile>> GetStudentsByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
    {
        return await dbContext.StudentProfiles.Where(p => authUserIds.Contains(p.AuthUserId)).ToListAsync(ct);
    }

    public async Task<TeacherProfile?> GetTeacherByUserIdAsync(Guid authUserId, CancellationToken ct = default)
    {
        return await dbContext.TeacherProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId, ct);
    }

    public async Task<List<TeacherProfile>> GetTeachersByUserIdsAsync(IEnumerable<Guid> authUserIds, CancellationToken ct = default)
    {
        return await dbContext.TeacherProfiles.Where(p => authUserIds.Contains(p.AuthUserId)).ToListAsync(ct);
    }

    public async Task<AdminProfile?> GetAdminByUserIdAsync(Guid authUserId, CancellationToken ct = default)
    {
        return await dbContext.AdminProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId, ct);
    }

    public async Task AddAsync(TeacherProfile profile, CancellationToken ct = default)
    {
        dbContext.TeacherProfiles.Add(profile);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task AddAsync(StudentProfile profile, CancellationToken ct = default)
    {
        dbContext.StudentProfiles.Add(profile);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task AddAsync(AdminProfile profile, CancellationToken ct = default)
    {
        dbContext.AdminProfiles.Add(profile);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(StudentProfile profile, CancellationToken ct = default)
    {
        dbContext.StudentProfiles.Update(profile);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TeacherProfile profile, CancellationToken ct = default)
    {
        dbContext.TeacherProfiles.Update(profile);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AdminProfile profile, CancellationToken ct = default)
    {
        dbContext.AdminProfiles.Update(profile);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteForUserAsync(Guid authUserId, CancellationToken ct = default)
    {
        var teacher = await dbContext.TeacherProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId, ct);
        teacher?.Delete();

        var student = await dbContext.StudentProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId, ct);
        student?.Delete();

        var admin = await dbContext.AdminProfiles.FirstOrDefaultAsync(p => p.AuthUserId == authUserId, ct);
        admin?.Delete();

        await dbContext.SaveChangesAsync(ct);
    }
}
