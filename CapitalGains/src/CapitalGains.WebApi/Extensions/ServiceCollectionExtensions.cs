namespace CapitalGains.WebApi.Extensions;

/// <summary>
/// Extension methods for registering Web API services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Web API specific services
    /// </summary>
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        // Add any Web API specific services here if needed
        return services;
    }
}