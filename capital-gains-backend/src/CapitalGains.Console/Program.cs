using CapitalGains.Application.Extensions;
using CapitalGains.Application.UseCases;
using CapitalGains.Infrastructure.Extensions;
using CapitalGains.Infrastructure.IO;
using CapitalGains.Infrastructure.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CapitalGains.Console;

public class CapitalGainsService : IHostedService
{
    private readonly IConsoleService _consoleService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IProcessCapitalGainsUseCase _processCapitalGainsUseCase;
    private readonly ILogger<CapitalGainsService> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public CapitalGainsService(
        IConsoleService consoleService,
        IJsonSerializer jsonSerializer,
        IProcessCapitalGainsUseCase processCapitalGainsUseCase,
        ILogger<CapitalGainsService> logger,
        IHostApplicationLifetime lifetime)
    {
        _consoleService = consoleService ?? throw new ArgumentNullException(nameof(consoleService));
        _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        _processCapitalGainsUseCase = processCapitalGainsUseCase ?? throw new ArgumentNullException(nameof(processCapitalGainsUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Capital Gains Calculator started");
            
            var inputLines = await _consoleService.ReadAllLinesAsync(cancellationToken);
            
            foreach (var line in inputLines)
            {
                await ProcessLineAsync(line, cancellationToken);
            }
            
            _logger.LogInformation("Capital Gains Calculator completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Capital Gains Calculator");
            Environment.ExitCode = 1;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Capital Gains Calculator stopped");
        return Task.CompletedTask;
    }

    private async Task ProcessLineAsync(string inputLine, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing input line: {InputLine}", inputLine);
            
            var operations = _jsonSerializer.DeserializeOperations(inputLine);
            
            var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);
            
            var outputJson = _jsonSerializer.SerializeTaxResults(results);
            await _consoleService.WriteLineAsync(outputJson, cancellationToken);
            
            _logger.LogDebug("Successfully processed line with {OperationCount} operations", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing input line: {InputLine}", inputLine);
            throw;
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Logging.ClearProviders();
        
#if DEBUG
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Error);
        builder.Logging.SetMinimumLevel(LogLevel.Error);
#endif

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices();
        
        builder.Services.AddHostedService<CapitalGainsService>();

        var host = builder.Build();
        
        await host.RunAsync();
    }
}