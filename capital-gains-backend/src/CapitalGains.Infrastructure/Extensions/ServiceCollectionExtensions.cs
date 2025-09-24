using CapitalGains.Infrastructure.IO;
using CapitalGains.Infrastructure.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalGains.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IJsonSerializer, JsonSerializer>();
        
        services.AddSingleton<IConsoleService, ConsoleService>();
        services.AddSingleton<IFileProcessor, FileProcessor>();
        
        return services;
    }
}