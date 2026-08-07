using AssignmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentSystem.Infrastructure.Persistence.Configurations;

public class AuthUserConfiguration : IEntityTypeConfiguration<AuthUser>
{
    public void Configure(EntityTypeBuilder<AuthUser> builder)
    {
        builder.Property(x => x.Role).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();
        builder.HasIndex(x => x.Email).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
