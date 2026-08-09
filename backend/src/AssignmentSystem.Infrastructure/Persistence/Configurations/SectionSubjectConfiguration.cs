using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class SectionSubjectConfiguration : IEntityTypeConfiguration<SectionSubject>
{
    public void Configure(EntityTypeBuilder<SectionSubject> builder)
    {
        builder.HasIndex(x => new { x.SectionId, x.SubjectId }).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
