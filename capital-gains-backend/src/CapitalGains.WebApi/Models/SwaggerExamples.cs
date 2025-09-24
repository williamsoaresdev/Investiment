using CapitalGains.Infrastructure.Serialization;

namespace CapitalGains.WebApi.Models;

public static class SwaggerExamples
{
    public static OperationsRequest CalculateRequestExample => new()
    {
        Operations = new[]
        {
            new OperationDto 
            { 
                Operation = "buy", 
                UnitCost = 10.00m, 
                Quantity = 100 
            },
            new OperationDto 
            { 
                Operation = "sell", 
                UnitCost = 15.00m, 
                Quantity = 50 
            },
            new OperationDto 
            { 
                Operation = "sell", 
                UnitCost = 15.00m, 
                Quantity = 50 
            }
        }
    };

    public static OperationsResponse CalculateResponseExample => new()
    {
        Taxes = new[]
        {
            new TaxResultDto { Tax = 0.00m },
            new TaxResultDto { Tax = 0.00m },
            new TaxResultDto { Tax = 0.00m }
        }
    };

    public static string FileContentExample => 
        @"[{""operation"":""buy"",""unit-cost"":10.00,""quantity"":100},{""operation"":""sell"",""unit-cost"":15.00,""quantity"":50}]";

    public static string MultiLineFileExample => 
        @"[{""operation"":""buy"",""unit-cost"":10.00,""quantity"":100},{""operation"":""sell"",""unit-cost"":15.00,""quantity"":50}]
[{""operation"":""buy"",""unit-cost"":20.00,""quantity"":200},{""operation"":""sell"",""unit-cost"":25.00,""quantity"":100}]";
}