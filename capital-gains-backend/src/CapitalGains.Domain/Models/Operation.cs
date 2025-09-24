namespace CapitalGains.Domain.Models;

public readonly record struct Operation(
    OperationType Type,
    decimal UnitCost,
    int Quantity)
{
    public decimal TotalValue => UnitCost * Quantity;

    public bool IsValid => UnitCost > 0 && Quantity > 0;
}

public enum OperationType
{
    Buy,
    Sell
}