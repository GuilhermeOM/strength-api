namespace Strength.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Strength.Domain.Entities;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        _ = builder.HasIndex(prop => prop.Name)
            .IsUnique();

        _ = builder
            .Property(property => property.Name)
            .HasConversion<string>()
            .IsRequired();
    }
}
