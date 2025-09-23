using CapitalGains.Infrastructure.IO;
using CapitalGains.Infrastructure.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalGains.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure layer services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register serialization services
        services.AddSingleton<IJsonSerializer, JsonSerializer>();
        
        // Register I/O services
        services.AddSingleton<IConsoleService, ConsoleService>();
        services.AddSingleton<IFileProcessor, FileProcessor>();
        
        return services;
    }
}