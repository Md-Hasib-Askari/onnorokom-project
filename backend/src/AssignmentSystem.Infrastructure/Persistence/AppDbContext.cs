using System.Linq.Expressions;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuthUser>().Property(x => x.Role).HasConversion<string>();
        modelBuilder.Entity<AuthUser>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Assignment>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<Submission>().Property(x => x.Status).HasConversion<string>();

        modelBuilder.Entity<Assignment>().Property(x => x.MaxMarks).HasPrecision(10, 2);
        modelBuilder.Entity<Submission>().Property(x => x.Marks).HasPrecision(10, 2);

        modelBuilder.Entity<AuthUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Grade>().HasIndex(x => new { x.Name, x.AcademicYear }).IsUnique();
        modelBuilder.Entity<Subject>().HasIndex(x => new { x.Code, x.GradeId }).IsUnique();

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
}
