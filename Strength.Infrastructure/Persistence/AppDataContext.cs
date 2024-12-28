namespace Strength.Infrastructure.Persistence;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Role> Roles { get; init; }
    public DbSet<UserRole> UserRoles { get; init; }
    public DbSet<Muscle> Muscles { get; init; }
    public DbSet<Exercise> Exercises { get; init; }
    public DbSet<MuscleExercise> MuscleExercises { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDataContext).Assembly);
}
