namespace CapitalGains.Domain.Models;

public readonly record struct TaxResult(decimal Tax)
{
    public decimal RoundedTax => Math.Round(Tax, 2, MidpointRounding.AwayFromZero);
}

public readonly record struct TaxResultCollection(IReadOnlyList<TaxResult> Results)
{
    public static TaxResultCollection Empty => new(Array.Empty<TaxResult>());
    
    public int Count => Results.Count;
    
    public TaxResult this[int index] => Results[index];
}