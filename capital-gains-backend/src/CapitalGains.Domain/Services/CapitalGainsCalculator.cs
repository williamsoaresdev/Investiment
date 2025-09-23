using CapitalGains.Domain.Models;

namespace CapitalGains.Domain.Services;

/// <summary>
/// Interface for capital gains tax calculation service
/// </summary>
public interface ICapitalGainsCalculator
{
    /// <summary>
    /// Processes a list of operations and returns tax results
    /// </summary>
    TaxResultCollection ProcessOperations(IEnumerable<Operation> operations);
}

/// <summary>
/// Service responsible for calculating capital gains taxes
/// </summary>
public class CapitalGainsCalculator : ICapitalGainsCalculator
{
    public TaxResultCollection ProcessOperations(IEnumerable<Operation> operations)
    {
        var portfolio = new Portfolio();
        var results = new List<TaxResult>();

        foreach (var operation in operations)
        {
            if (!operation.IsValid)
                throw new ArgumentException($"Invalid operation: {operation}");

            var taxResult = operation.Type switch
            {
                OperationType.Buy => ProcessBuyOperation(portfolio, operation),
                OperationType.Sell => ProcessSellOperation(portfolio, operation),
                _ => throw new ArgumentException($"Unknown operation type: {operation.Type}")
            };

            results.Add(taxResult);
        }

        return new TaxResultCollection(results);
    }

    private static TaxResult ProcessBuyOperation(Portfolio portfolio, Operation operation)
    {
        portfolio.Buy(operation.UnitCost, operation.Quantity);
        return new TaxResult(0m); // Buy operations never generate tax
    }

    private static TaxResult ProcessSellOperation(Portfolio portfolio, Operation operation)
    {
        return portfolio.Sell(operation.UnitCost, operation.Quantity);
    }
}