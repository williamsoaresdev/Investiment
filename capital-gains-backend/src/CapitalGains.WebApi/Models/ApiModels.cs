using CapitalGains.Infrastructure.Serialization;
using CapitalGains.Domain.Models;
using System.Text.Json.Serialization;

namespace CapitalGains.WebApi.Models;

/// <summary>
/// Request model for capital gains operations
/// </summary>
public class OperationsRequest
{
    /// <summary>
    /// List of operations to process
    /// </summary>
    public IEnumerable<OperationDto> Operations { get; set; } = Enumerable.Empty<OperationDto>();
}

/// <summary>
/// Response model for capital gains calculations
/// </summary>
public class OperationsResponse
{
    /// <summary>
    /// Tax calculation results
    /// </summary>
    public IEnumerable<TaxResultDto> Taxes { get; set; } = Enumerable.Empty<TaxResultDto>();
}

/// <summary>
/// Enhanced response model that includes both input operations and tax results
/// </summary>
public class CalculationResultResponse
{
    /// <summary>
    /// Original operations that were processed
    /// </summary>
    public IEnumerable<OperationDto> Operations { get; set; } = Enumerable.Empty<OperationDto>();

    /// <summary>
    /// Enhanced tax calculation results that can include errors
    /// </summary>
    public IEnumerable<EnhancedTaxResultDto> Taxes { get; set; } = Enumerable.Empty<EnhancedTaxResultDto>();
    
    /// <summary>
    /// Information about scenarios processed (for file uploads with multiple lines)
    /// </summary>
    public IEnumerable<ScenarioInfo> Scenarios { get; set; } = Enumerable.Empty<ScenarioInfo>();
}

/// <summary>
/// Information about a processed scenario from file upload
/// </summary>
public class ScenarioInfo
{
    /// <summary>
    /// Scenario number/line number
    /// </summary>
    [JsonPropertyName("scenarioNumber")]
    public int ScenarioNumber { get; set; }
    
    /// <summary>
    /// Number of operations in this scenario
    /// </summary>
    [JsonPropertyName("operationCount")]
    public int OperationCount { get; set; }
    
    /// <summary>
    /// Number of results for this scenario
    /// </summary>
    [JsonPropertyName("resultCount")]
    public int ResultCount { get; set; }
    
    /// <summary>
    /// Starting index in the operations array
    /// </summary>
    [JsonPropertyName("operationStartIndex")]
    public int OperationStartIndex { get; set; }
    
    /// <summary>
    /// Starting index in the results array
    /// </summary>
    [JsonPropertyName("resultStartIndex")]
    public int ResultStartIndex { get; set; }
}

/// <summary>
/// Enhanced tax result model that can include error information
/// </summary>
public class EnhancedTaxResultDto
{
    /// <summary>
    /// Tax amount calculated
    /// </summary>
    [JsonPropertyName("tax")]
    public decimal? Tax { get; set; }

    /// <summary>
    /// Error message if calculation failed
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Indicates if this result has an error
    /// </summary>
    [JsonPropertyName("hasError")]
    public bool HasError { get; set; }

    /// <summary>
    /// Creates DTO from domain model
    /// </summary>
    public static EnhancedTaxResultDto FromDomain(TaxResult taxResult)
    {
        return new EnhancedTaxResultDto 
        { 
            Tax = taxResult.RoundedTax, 
            HasError = false 
        };
    }

    /// <summary>
    /// Creates error DTO
    /// </summary>
    public static EnhancedTaxResultDto FromError(string errorMessage)
    {
        return new EnhancedTaxResultDto 
        { 
            Error = errorMessage, 
            HasError = true 
        };
    }
}