using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.Property(x => x.Key).HasConversion<string>().HasMaxLength(100);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Key).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
