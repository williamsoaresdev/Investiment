namespace CapitalGains.Domain.Models;

public readonly record struct TaxResult
{
    public decimal Tax { get; }
    public string? Error { get; }
    public bool HasError => !string.IsNullOrEmpty(Error);

    public TaxResult(decimal tax)
    {
        Tax = tax;
        Error = null;
    }

    public TaxResult(string error)
    {
        Tax = 0m;
        Error = error;
    }

    public decimal RoundedTax => Math.Round(Tax, 2, MidpointRounding.AwayFromZero);
}

public readonly record struct TaxResultCollection(IReadOnlyList<TaxResult> Results)
{
    public static TaxResultCollection Empty => new(Array.Empty<TaxResult>());
    
    public int Count => Results.Count;
    
    public TaxResult this[int index] => Results[index];
}