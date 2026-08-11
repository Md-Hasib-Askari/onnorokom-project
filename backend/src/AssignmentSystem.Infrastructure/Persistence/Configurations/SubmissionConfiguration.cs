using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.Marks).HasPrecision(10, 2);

        // One submission row per student per assignment: a revision overwrites in place rather
        // than adding an attempt. Filtered so a soft-deleted row does not block a fresh one.
        builder.HasIndex(x => new { x.AssignmentId, x.StudentId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
