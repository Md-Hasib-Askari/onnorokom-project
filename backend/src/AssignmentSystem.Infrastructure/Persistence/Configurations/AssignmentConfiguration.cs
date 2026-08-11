using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.MaxMarks).HasPrecision(10, 2);

        // The two list queries this feature adds are "assignments for a section" (student) and
        // "assignments I authored" (teacher); both filter on these columns alone.
        builder.HasIndex(x => x.SectionId);
        builder.HasIndex(x => x.TeacherId);
    }
}
