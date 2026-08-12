using System.Linq.Expressions;
using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : DbContext(options)
{
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SectionSubject> SectionSubjects => Set<SectionSubject>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuthUserConfiguration());
        modelBuilder.ApplyConfiguration(new GradeConfiguration());
        modelBuilder.ApplyConfiguration(new SectionConfiguration());
        modelBuilder.ApplyConfiguration(new SubjectConfiguration());
        modelBuilder.ApplyConfiguration(new SectionSubjectConfiguration());
        modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new SubmissionConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetCodeConfiguration());

        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeleted = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(isDeleted, Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            Action apply = entry switch
            {
                { State: EntityState.Added, Entity: ICreatable creatable } => () =>
                {
                    creatable.CreatedAt = now;
                    if (string.IsNullOrWhiteSpace(creatable.CreatedBy))
                    {
                        creatable.CreatedBy = userId;
                    }

                    if (creatable is IUpdatable updatable)
                    {
                        updatable.UpdatedAt = now;
                        updatable.UpdatedBy = userId;
                    }
                },
                { State: EntityState.Modified, Entity: ISoftDeletable deletable } when deletable.IsDeleted => () =>
                    ApplySoftDelete(deletable, now, userId),
                { State: EntityState.Modified, Entity: IUpdatable updatable } => () =>
                {
                    updatable.UpdatedAt = now;
                    updatable.UpdatedBy = userId;
                },
                { State: EntityState.Deleted, Entity: ISoftDeletable softDeletable } => () =>
                {
                    ApplySoftDelete(softDeletable, now, userId);
                    softDeletable.IsDeleted = true;
                    entry.State = EntityState.Modified;
                },
                _ => () => { }
            };

            apply();
        }
    }

    private static void ApplySoftDelete(ISoftDeletable entity, DateTimeOffset now, string? userId)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = now;
        entity.DeletedBy = userId;
    }
}
