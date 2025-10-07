using CapitalGains.Infrastructure.Serialization;
using CapitalGains.Domain.Models;
using System.Text.Json.Serialization;

namespace CapitalGains.WebApi.Models;

public class OperationsRequest
{
    public IEnumerable<OperationDto> Operations { get; set; } = Enumerable.Empty<OperationDto>();
}

public class OperationsResponse
{
    public IEnumerable<TaxResultDto> Taxes { get; set; } = Enumerable.Empty<TaxResultDto>();
}

public class CalculationResultResponse
{
    public IEnumerable<OperationDto> Operations { get; set; } = Enumerable.Empty<OperationDto>();
    public IEnumerable<EnhancedTaxResultDto> Taxes { get; set; } = Enumerable.Empty<EnhancedTaxResultDto>();
    public IEnumerable<ScenarioInfo> Scenarios { get; set; } = Enumerable.Empty<ScenarioInfo>();
}

public class ScenarioInfo
{
    [JsonPropertyName("scenarioNumber")]
    public int ScenarioNumber { get; set; }
    
    [JsonPropertyName("operationCount")]
    public int OperationCount { get; set; }
    
    [JsonPropertyName("resultCount")]
    public int ResultCount { get; set; }
    
    [JsonPropertyName("operationStartIndex")]
    public int OperationStartIndex { get; set; }
    
    [JsonPropertyName("resultStartIndex")]
    public int ResultStartIndex { get; set; }
}

public class EnhancedTaxResultDto
{
    [JsonPropertyName("tax")]
    public decimal? Tax { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("hasError")]
    public bool HasError { get; set; }

    public static EnhancedTaxResultDto FromDomain(TaxResult taxResult)
    {
        if (taxResult.HasError)
        {
            return new EnhancedTaxResultDto 
            { 
                Error = taxResult.Error, 
                HasError = true 
            };
        }

        return new EnhancedTaxResultDto 
        { 
            Tax = taxResult.RoundedTax, 
            HasError = false 
        };
    }

    public static EnhancedTaxResultDto FromError(string errorMessage)
    {
        return new EnhancedTaxResultDto 
        { 
            Error = errorMessage, 
            HasError = true 
        };
    }
}