namespace Strength.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MuscleConfiguration : IEntityTypeConfiguration<Muscle>
{
    public void Configure(EntityTypeBuilder<Muscle> builder) => builder.HasIndex(prop => prop.Name).IsUnique();
}
