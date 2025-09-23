namespace CapitalGains.Domain.Models;

/// <summary>
/// Represents a financial operation (buy or sell)
/// </summary>
public readonly record struct Operation(
    OperationType Type,
    decimal UnitCost,
    int Quantity)
{
    /// <summary>
    /// Calculates the total value of the operation
    /// </summary>
    public decimal TotalValue => UnitCost * Quantity;

    /// <summary>
    /// Validates if the operation has valid values
    /// </summary>
    public bool IsValid => UnitCost > 0 && Quantity > 0;
}

/// <summary>
/// Types of operations supported
/// </summary>
public enum OperationType
{
    Buy,
    Sell
}