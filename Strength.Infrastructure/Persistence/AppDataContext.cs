namespace Strength.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Strength.Domain.Entities;

public class AppDataContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDataContext).Assembly);

    public DbSet<User> Users { get; init; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Role> Roles { get; set; }
}
