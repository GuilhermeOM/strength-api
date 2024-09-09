namespace Strength.Application;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Strength.Application.Abstractions.Behaviors;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        _ = services.AddMediatR(config =>
                config.RegisterServicesFromAssembly(assembly));

        _ = services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        _ = services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

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
