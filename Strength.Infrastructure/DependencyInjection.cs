namespace Strength.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Domain.Repositories;
using Domain.Shared;
using Persistence;
using Persistence.Repositories;
using Security;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddSingleton<ITokenProvider, TokenProvider>(_ => new TokenProvider(configuration));

        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        _ = services.AddScoped<IRoleRepository, RoleRepository>();

        var connectionString = configuration.GetConnectionString("DatabaseConnection");

        _ = services.AddDbContext<AppDataContext>(options =>
                options.UseSqlServer(connectionString));

        return services;
    }
}
