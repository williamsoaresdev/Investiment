using CapitalGains.Application.UseCases;
using CapitalGains.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalGains.Application.Extensions;

/// <summary>
/// Extension methods for registering application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application layer services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<ICapitalGainsCalculator, CapitalGainsCalculator>();
        
        // Register use cases
        services.AddScoped<IProcessCapitalGainsUseCase, ProcessCapitalGainsUseCase>();
        
        return services;
    }
}