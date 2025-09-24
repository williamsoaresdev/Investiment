using System.Text.Json;
using System.Text.Json.Serialization;
using CapitalGains.Domain.Models;

namespace CapitalGains.Infrastructure.Serialization;

public class OperationDto
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("unit-cost")]
    public decimal UnitCost { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

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

    public static OperationDto FromDomain(Operation operation)
    {
        return new OperationDto
        {
            Operation = operation.Type.ToString().ToLowerInvariant(),
            UnitCost = operation.UnitCost,
            Quantity = operation.Quantity
        };
    }
}

public class TaxResultDto
{
    [JsonPropertyName("tax")]
    public decimal Tax { get; set; }

    public static TaxResultDto FromDomain(TaxResult taxResult)
    {
        return new TaxResultDto { Tax = taxResult.RoundedTax };
    }
}