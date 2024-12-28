namespace Strength.Infrastructure.Persistence.Configurations;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MuscleExerciseConfiguration : IEntityTypeConfiguration<MuscleExercise>
{
    public void Configure(EntityTypeBuilder<MuscleExercise> builder)
    {
        builder
            .HasOne(me => me.Muscle)
            .WithMany(m => m.MuscleExercises)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(me => me.Exercise)
            .WithMany(e => e.MuscleExercises)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
