using System.Text.Json;
using System.Text.Json.Serialization;
using CapitalGains.Domain.Models;

namespace CapitalGains.Infrastructure.Serialization;

/// <summary>
/// JSON representation of an operation for serialization
/// </summary>
public class OperationDto
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("unit-cost")]
    public decimal UnitCost { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Converts DTO to domain model
    /// </summary>
    public Operation ToDomain()
    {
        var operationType = Operation.ToLowerInvariant() switch
        {
            "buy" => OperationType.Buy,
            "sell" => OperationType.Sell,
            _ => throw new ArgumentException($"Unknown operation type: {Operation}")
        };

        return new Operation(operationType, UnitCost, Quantity);
    }
}

/// <summary>
/// JSON representation of a tax result for serialization
/// </summary>
public class TaxResultDto
{
    [JsonPropertyName("tax")]
    public decimal Tax { get; set; }

    /// <summary>
    /// Creates DTO from domain model
    /// </summary>
    public static TaxResultDto FromDomain(TaxResult taxResult)
    {
        return new TaxResultDto { Tax = taxResult.RoundedTax };
    }
}