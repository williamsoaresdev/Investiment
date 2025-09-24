using CapitalGains.Domain.Models;

namespace CapitalGains.Domain.Services;

public interface ICapitalGainsCalculator
{
    TaxResultCollection ProcessOperations(IEnumerable<Operation> operations);
}

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
        return new TaxResult(0m);
    }

    private static TaxResult ProcessSellOperation(Portfolio portfolio, Operation operation)
    {
        return portfolio.Sell(operation.UnitCost, operation.Quantity);
    }
}