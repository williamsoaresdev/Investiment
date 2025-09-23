using CapitalGains.Infrastructure.Serialization;

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