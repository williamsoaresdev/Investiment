using CapitalGains.Application.UseCases;
using CapitalGains.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CapitalGains.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICapitalGainsCalculator, CapitalGainsCalculator>();

        services.AddScoped<IProcessCapitalGainsUseCase, ProcessCapitalGainsUseCase>();        return services;
    }
}