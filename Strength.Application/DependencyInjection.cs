namespace Strength.Application;

using Abstractions.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        _ = services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

        _ = services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        _ = services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
