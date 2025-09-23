namespace CapitalGains.Domain.Models;

/// <summary>
/// Represents the current state of a stock portfolio
/// </summary>
public class Portfolio
{
    private decimal _averagePrice;
    private int _quantity;
    private decimal _accumulatedLoss;

    public Portfolio()
    {
        _averagePrice = 0m;
        _quantity = 0;
        _accumulatedLoss = 0m;
    }

    /// <summary>
    /// Current weighted average price of stocks
    /// </summary>
    public decimal AveragePrice => _averagePrice;

    /// <summary>
    /// Current quantity of stocks owned
    /// </summary>
    public int Quantity => _quantity;

    /// <summary>
    /// Accumulated losses that can be deducted from future profits
    /// </summary>
    public decimal AccumulatedLoss => _accumulatedLoss;

    /// <summary>
    /// Checks if the portfolio has any stocks
    /// </summary>
    public bool HasStocks => _quantity > 0;

    /// <summary>
    /// Processes a buy operation, updating the weighted average price
    /// </summary>
    public void Buy(decimal unitCost, int quantity)
    {
        if (unitCost <= 0 || quantity <= 0)
            throw new ArgumentException("Unit cost and quantity must be positive");

        var totalCurrentValue = _averagePrice * _quantity;
        var totalNewValue = unitCost * quantity;
        var totalQuantity = _quantity + quantity;

        _averagePrice = totalQuantity > 0 ? (totalCurrentValue + totalNewValue) / totalQuantity : 0m;
        _quantity = totalQuantity;
    }

    /// <summary>
    /// Processes a sell operation and calculates tax
    /// </summary>
    public TaxResult Sell(decimal unitCost, int quantity)
    {
        if (unitCost <= 0 || quantity <= 0)
            throw new ArgumentException("Unit cost and quantity must be positive");
        
        if (quantity > _quantity)
            throw new InvalidOperationException("Cannot sell more stocks than owned");

        var totalOperationValue = unitCost * quantity;
        var totalCostBasis = _averagePrice * quantity;
        var profitOrLoss = totalOperationValue - totalCostBasis;
        
        // Update quantity
        _quantity -= quantity;
        
        // If no stocks left, reset average price
        if (_quantity == 0)
            _averagePrice = 0m;

        // Calculate tax
        var tax = CalculateTax(profitOrLoss, totalOperationValue);
        
        return new TaxResult(tax);
    }

    private decimal CalculateTax(decimal profitOrLoss, decimal totalOperationValue)
    {
        const decimal taxRate = 0.20m;
        const decimal exemptionThreshold = 20000m;

        // No tax on operations <= 20,000
        if (totalOperationValue <= exemptionThreshold)
        {
            // Still accumulate losses for future deduction
            if (profitOrLoss < 0)
                _accumulatedLoss += Math.Abs(profitOrLoss);
            
            return 0m;
        }

        // Handle losses
        if (profitOrLoss <= 0)
        {
            _accumulatedLoss += Math.Abs(profitOrLoss);
            return 0m;
        }

        // Handle profits
        var taxableProfit = profitOrLoss;
        
        // Deduct accumulated losses
        if (_accumulatedLoss > 0)
        {
            var lossToDeduct = Math.Min(_accumulatedLoss, taxableProfit);
            taxableProfit -= lossToDeduct;
            _accumulatedLoss -= lossToDeduct;
        }

        return taxableProfit > 0 ? Math.Round(taxableProfit * taxRate, 2, MidpointRounding.AwayFromZero) : 0m;
    }

    /// <summary>
    /// Resets the portfolio to initial state
    /// </summary>
    public void Reset()
    {
        _averagePrice = 0m;
        _quantity = 0;
        _accumulatedLoss = 0m;
    }
}