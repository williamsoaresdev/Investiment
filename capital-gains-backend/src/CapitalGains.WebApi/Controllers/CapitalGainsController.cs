using CapitalGains.Application.UseCases;
using CapitalGains.Infrastructure.Serialization;
using CapitalGains.Infrastructure.IO;
using CapitalGains.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CapitalGains.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Capital Gains")]
public class CapitalGainsController : ControllerBase
{
    private readonly IProcessCapitalGainsUseCase _processCapitalGainsUseCase;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IFileProcessor _fileProcessor;
    private readonly ILogger<CapitalGainsController> _logger;

    public CapitalGainsController(
        IProcessCapitalGainsUseCase processCapitalGainsUseCase,
        IJsonSerializer jsonSerializer,
        IFileProcessor fileProcessor,
        ILogger<CapitalGainsController> logger)
    {
        _processCapitalGainsUseCase = processCapitalGainsUseCase;
        _jsonSerializer = jsonSerializer;
        _fileProcessor = fileProcessor;
        _logger = logger;
    }

    [HttpPost("calculate")]
    [ProducesResponseType(typeof(CalculationResultResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<CalculationResultResponse>> CalculateCapitalGains(
        [FromBody] OperationsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing capital gains calculation request with {OperationCount} operations", 
                request.Operations.Count());

            if (request.Operations == null || !request.Operations.Any())
            {
                return BadRequest("Operations list cannot be empty");
            }

            var allOperations = new List<OperationDto>();
            var allResults = new List<EnhancedTaxResultDto>();
            
            var requestOperations = request.Operations.ToList();
            allOperations.AddRange(requestOperations);

            try
            {
                var operations = new List<Domain.Models.Operation>();
                var hasInvalidOperations = false;

                foreach (var dto in requestOperations)
                {
                    try
                    {
                        var operation = dto.ToDomain();
                        if (!operation.IsValid)
                        {
                            allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation: negative values not allowed"));
                            hasInvalidOperations = true;
                        }
                        else
                        {
                            operations.Add(operation);
                            allResults.Add(null!);
                        }
                    }
                    catch (ArgumentException)
                    {
                        allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation type"));
                        hasInvalidOperations = true;
                    }
                }

                if (operations.Any() && !hasInvalidOperations)
                {
                    var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);
                    
                    var resultIndex = 0;
                    for (int i = 0; i < allResults.Count; i++)
                    {
                        if (allResults[i] == null)
                        {
                            allResults[i] = EnhancedTaxResultDto.FromDomain(results.Results[resultIndex]);
                            resultIndex++;
                        }
                    }
                }
                else if (operations.Any())
                {
                    try
                    {
                        var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);
                        
                        var resultIndex = 0;
                        for (int i = 0; i < allResults.Count; i++)
                        {
                            if (allResults[i] == null && resultIndex < results.Results.Count())
                            {
                                allResults[i] = EnhancedTaxResultDto.FromDomain(results.Results.ElementAt(resultIndex));
                                resultIndex++;
                            }
                            else if (allResults[i] == null)
                            {
                                allResults[i] = EnhancedTaxResultDto.FromError("Processing error");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing valid operations");
                        for (int i = 0; i < allResults.Count; i++)
                        {
                            if (allResults[i] == null)
                            {
                                allResults[i] = EnhancedTaxResultDto.FromError("Processing error");
                            }
                        }
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing operations");
                for (int i = 0; i < requestOperations.Count; i++)
                {
                    if (i < allResults.Count && allResults[i] == null)
                    {
                        allResults[i] = EnhancedTaxResultDto.FromError("Processing error");
                    }
                    else if (i >= allResults.Count)
                    {
                        allResults.Add(EnhancedTaxResultDto.FromError("Processing error"));
                    }
                }
            }

            var response = new CalculationResultResponse
            {
                Operations = allOperations,
                Taxes = allResults.Where(r => r != null!),
                Scenarios = Enumerable.Empty<ScenarioInfo>()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing capital gains request");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(CalculationResultResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<CalculationResultResponse>> UploadFile(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing file upload: {FileName}", file?.FileName ?? "unknown");

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty");
            }

            var allowedExtensions = new[] { ".json", ".txt" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Only .json and .txt files are supported");
            }

            string fileContent;
            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return BadRequest("File is empty or contains only whitespace");
            }

            _logger.LogInformation("File content read successfully. Content length: {Length} characters", fileContent.Length);

            var processedData = _fileProcessor.ProcessFileContent(fileContent);

            if (!processedData.Any())
            {
                return BadRequest("No valid JSON content found in file");
            }

            var allOperations = new List<OperationDto>();
            var allResults = new List<EnhancedTaxResultDto>();
            var scenarios = new List<ScenarioInfo>();

            foreach (var (lineNumber, jsonLine) in processedData.Select((line, index) => (index + 1, line)))
            {
                try
                {
                    var operations = _jsonSerializer.DeserializeOperations(jsonLine);
                    
                    if (operations == null || !operations.Any())
                    {
                        continue;
                    }

                    var scenarioInfo = new ScenarioInfo
                    {
                        ScenarioNumber = lineNumber,
                        OperationStartIndex = allOperations.Count
                    };

                    foreach (var operation in operations)
                    {
                        try
                        {
                            if (!operation.IsValid)
                            {
                                allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation: negative values not allowed"));
                            }
                        }
                        catch
                        {
                            allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation format"));
                        }
                    }

                    var operationDtos = operations.Select(OperationDto.FromDomain);
                    allOperations.AddRange(operationDtos);

                    try
                    {
                        var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);
                        
                        allResults.AddRange(results.Results.Select(EnhancedTaxResultDto.FromDomain));
                        scenarioInfo.ResultCount = results.Results.Count();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing operations in line {LineNumber}", lineNumber);
                        
                        for (int i = 0; i < operations.Count(); i++)
                        {
                            allResults.Add(EnhancedTaxResultDto.FromError($"Processing error: {ex.Message}"));
                        }
                        scenarioInfo.ResultCount = operations.Count();
                    }

                    scenarioInfo.OperationCount = operations.Count();
                    scenarioInfo.ResultStartIndex = allResults.Count - scenarioInfo.ResultCount;
                    scenarios.Add(scenarioInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse line {LineNumber}: {JsonLine}", lineNumber, jsonLine);
                    continue;
                }
            }

            var response = new CalculationResultResponse
            {
                Operations = allOperations,
                Taxes = allResults,
                Scenarios = scenarios
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file upload");
            return StatusCode(500, "Internal server error");
        }
    }
}