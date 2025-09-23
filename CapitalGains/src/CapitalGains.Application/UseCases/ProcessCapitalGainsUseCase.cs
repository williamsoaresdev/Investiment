using CapitalGains.Domain.Models;
using CapitalGains.Domain.Services;
using Microsoft.Extensions.Logging;

namespace CapitalGains.Application.UseCases;

/// <summary>
/// Use case for processing capital gains operations
/// </summary>
public interface IProcessCapitalGainsUseCase
{
    /// <summary>
    /// Processes a batch of operations and returns tax calculations
    /// </summary>
    Task<TaxResultCollection> ExecuteAsync(IEnumerable<Operation> operations, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of the process capital gains use case
/// </summary>
public class ProcessCapitalGainsUseCase : IProcessCapitalGainsUseCase
{
    private readonly ICapitalGainsCalculator _calculator;
    private readonly ILogger<ProcessCapitalGainsUseCase> _logger;

    public ProcessCapitalGainsUseCase(
        ICapitalGainsCalculator calculator,
        ILogger<ProcessCapitalGainsUseCase> logger)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<TaxResultCollection> ExecuteAsync(IEnumerable<Operation> operations, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Processing capital gains operations");
            
            var operationsList = operations.ToList();
            _logger.LogDebug("Processing {OperationCount} operations", operationsList.Count);
            
            var results = _calculator.ProcessOperations(operationsList);
            
            _logger.LogDebug("Successfully processed {OperationCount} operations, generated {ResultCount} tax results", 
                operationsList.Count, results.Count);
            
            return Task.FromResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing capital gains operations");
            throw;
        }
    }
}