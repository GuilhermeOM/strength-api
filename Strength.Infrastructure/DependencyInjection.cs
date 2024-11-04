namespace Strength.Infrastructure;

using Domain.Repositories;
using Domain.Services.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Repositories;
using Services;
using Strength.Domain.Services.Email;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
        .AddFluentEmail(configuration)
        .AddSqlServer(configuration)
        .AddServices(configuration)
        .AddRepositories()
        .AddMemoryCache();

    private static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DatabaseConnection");

        _ = services.AddDbContext<AppDataContext>(options => options.UseSqlServer(connectionString));

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddSingleton<ITokenService, TokenService>(_ => new TokenService(configuration));

        _ = services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        _ = services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }

    private static IServiceCollection AddFluentEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var smtpPortIsConfigured = int.TryParse(configuration["Email:Port"], out var smtpPort);

        if (!smtpPortIsConfigured)
        {
            throw new InvalidOperationException("SMTP port is not well configured");
        }

        _ = services
            .AddFluentEmail(configuration["Email:SenderEmail"], configuration["Email:Sender"])
            .AddSmtpSender(configuration["Email:Host"], smtpPort);

        return services;
    }
}
