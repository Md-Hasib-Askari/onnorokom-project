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

    public async Task UpdateAsync(StudentProfile profile, CancellationToken ct = default)
    {
        dbContext.StudentProfiles.Update(profile);
        await dbContext.SaveChangesAsync(ct);
    }
}
