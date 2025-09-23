using CapitalGains.Infrastructure.Serialization;

namespace CapitalGains.WebApi.Models;

/// <summary>
/// Exemplos para documentação do Swagger
/// </summary>
public static class SwaggerExamples
{
    /// <summary>
    /// Exemplo de request para cálculo de impostos
    /// </summary>
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

    /// <summary>
    /// Exemplo de response para cálculo de impostos
    /// </summary>
    public static OperationsResponse CalculateResponseExample => new()
    {
        Taxes = new[]
        {
            new TaxResultDto { Tax = 0.00m },
            new TaxResultDto { Tax = 0.00m },
            new TaxResultDto { Tax = 0.00m }
        }
    };

    /// <summary>
    /// Exemplo de conteúdo de arquivo JSON para upload
    /// </summary>
    public static string FileContentExample => 
        @"[{""operation"":""buy"",""unit-cost"":10.00,""quantity"":100},{""operation"":""sell"",""unit-cost"":15.00,""quantity"":50}]";

    /// <summary>
    /// Exemplo de arquivo TXT com múltiplas linhas
    /// </summary>
    public static string MultiLineFileExample => 
        @"[{""operation"":""buy"",""unit-cost"":10.00,""quantity"":100},{""operation"":""sell"",""unit-cost"":15.00,""quantity"":50}]
[{""operation"":""buy"",""unit-cost"":20.00,""quantity"":200},{""operation"":""sell"",""unit-cost"":25.00,""quantity"":100}]";
}