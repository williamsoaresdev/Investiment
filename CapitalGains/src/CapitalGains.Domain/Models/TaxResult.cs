namespace CapitalGains.Domain.Models;

/// <summary>
/// Represents the result of a tax calculation for an operation
/// </summary>
public readonly record struct TaxResult(decimal Tax)
{
    /// <summary>
    /// Rounds the tax value to 2 decimal places
    /// </summary>
    public decimal RoundedTax => Math.Round(Tax, 2, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Collection of tax results for multiple operations
/// </summary>
public readonly record struct TaxResultCollection(IReadOnlyList<TaxResult> Results)
{
    public static TaxResultCollection Empty => new(Array.Empty<TaxResult>());
    
    public int Count => Results.Count;
    
    public TaxResult this[int index] => Results[index];
}