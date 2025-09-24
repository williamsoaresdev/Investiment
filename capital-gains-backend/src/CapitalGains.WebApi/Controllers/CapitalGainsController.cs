using CapitalGains.Application.UseCases;
using CapitalGains.Infrastructure.Serialization;
using CapitalGains.Infrastructure.IO;
using CapitalGains.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CapitalGains.WebApi.Controllers;

/// <summary>
/// Controller para cálculos de impostos sobre ganhos de capital
/// </summary>
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

    /// <summary>
    /// Calcula impostos sobre ganhos de capital para uma lista de operações
    /// </summary>
    /// <param name="request">Lista de operações de compra e venda para processamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultados dos cálculos de impostos para cada operação</returns>
    /// <response code="200">Cálculo realizado com sucesso</response>
    /// <response code="400">Dados inválidos na requisição</response>
    /// <response code="500">Erro interno do servidor</response>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/capitalgains/calculate
    ///     {
    ///       "operations": [
    ///         {"operation": "buy", "unit-cost": 10.00, "quantity": 100},
    ///         {"operation": "sell", "unit-cost": 15.00, "quantity": 50}
    ///       ]
    ///     }
    ///     
    /// Regras de negócio:
    /// - Operações de compra nunca geram impostos
    /// - Impostos são calculados apenas sobre lucros de vendas
    /// - Perdas podem ser deduzidas de lucros futuros
    /// - Vendas até R$ 20.000,00 não geram impostos
    /// </remarks>
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

            // Validate request
            if (request.Operations == null || !request.Operations.Any())
            {
                return BadRequest("Operations list cannot be empty");
            }

            // Process each operation individually to handle errors
            var allOperations = new List<OperationDto>();
            var allResults = new List<EnhancedTaxResultDto>();
            
            // Convert request operations to a list for processing
            var requestOperations = request.Operations.ToList();
            allOperations.AddRange(requestOperations);

            try
            {
                // Try to convert DTOs to domain models
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
                            allResults.Add(null!); // Placeholder for valid operations
                        }
                    }
                    catch (ArgumentException)
                    {
                        allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation type"));
                        hasInvalidOperations = true;
                    }
                }

                // If we have valid operations, process them
                if (operations.Any() && !hasInvalidOperations)
                {
                    var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);
                    
                    // Replace placeholders with actual results
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
                    // Process valid operations even if some are invalid
                    try
                    {
                        var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);
                        
                        // Map results back to the correct positions
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
                        // Fill remaining null results with errors
                        for (int i = 0; i < allResults.Count; i++)
                        {
                            if (allResults[i] == null)
                            {
                                allResults[i] = EnhancedTaxResultDto.FromError("Processing error: " + ex.Message);
                            }
                        }
                    }
                }
                else
                {
                    // All operations are invalid - results are already set above
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing operations");
                // Fill all results with errors if something goes wrong
                allResults.Clear();
                for (int i = 0; i < requestOperations.Count; i++)
                {
                    allResults.Add(EnhancedTaxResultDto.FromError("Processing error: " + ex.Message));
                }
            }

            // Convert results to response DTOs
            var response = new CalculationResultResponse
            {
                Operations = allOperations,
                Taxes = allResults
            };

            _logger.LogInformation("Successfully processed {OperationCount} operations, generated {ResultCount} tax results",
                allOperations.Count, allResults.Count);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing capital gains calculation");
            return StatusCode(500, "Internal server error occurred while processing the request");
        }
    }

    /// <summary>
    /// Faz upload de um arquivo contendo operações para calcular impostos
    /// </summary>
    /// <param name="file">Arquivo (.txt ou .json) contendo as operações</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultados dos cálculos de impostos para todas as operações do arquivo</returns>
    /// <response code="200">Arquivo processado com sucesso</response>
    /// <response code="400">Arquivo inválido ou formato não suportado</response>
    /// <response code="500">Erro interno do servidor</response>
    /// <remarks>
    /// Formatos de arquivo suportados:
    /// 
    /// **Arquivo JSON (.json):**
    /// ```
    /// [{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50}]
    /// ```
    /// 
    /// **Arquivo TXT (.txt) - uma linha por conjunto de operações:**
    /// ```
    /// [{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50}]
    /// [{"operation":"buy","unit-cost":20.00,"quantity":200},{"operation":"sell","unit-cost":25.00,"quantity":100}]
    /// ```
    /// 
    /// Cada linha do arquivo TXT deve conter um array JSON válido com operações.
    /// </remarks>
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
            _logger.LogInformation("Processing file upload: {FileName}", file?.FileName);

            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required and cannot be empty");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".txt" && extension != ".json")
            {
                return BadRequest("Only .txt and .json files are supported");
            }

            // Read file content
            string content;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);
                content = Encoding.UTF8.GetString(stream.ToArray());
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("File content cannot be empty");
            }

            _logger.LogDebug("File content: {Content}", content);

            // Process file content and extract valid JSON lines
            var validJsonLines = _fileProcessor.ProcessFileContent(content).ToList();
            
            if (!validJsonLines.Any())
            {
                return BadRequest("No valid JSON operations found in file");
            }

            _logger.LogInformation("Found {ValidLineCount} valid JSON lines in uploaded file", validJsonLines.Count);

            // Process file content - each line is processed independently
            var allResults = new List<EnhancedTaxResultDto>();
            var allOperations = new List<OperationDto>();
            var scenarios = new List<ScenarioInfo>();

            _logger.LogInformation("Processing {LineCount} JSON lines from file", validJsonLines.Count);
            
            foreach (var (line, lineIndex) in validJsonLines.Select((line, index) => (line, index)))
            {
                try
                {
                    _logger.LogInformation("Processing line #{LineIndex}: {Line}", lineIndex + 1, line);
                    
                    // Parse operations from JSON line
                    var operations = _jsonSerializer.DeserializeOperations(line);
                    
                    // Validate operations
                    var operationsList = operations.ToList();
                    var invalidOperations = operationsList.Where(op => !op.IsValid).ToList();
                    
                    // Record scenario info
                    var scenarioInfo = new ScenarioInfo
                    {
                        ScenarioNumber = lineIndex + 1,
                        OperationCount = operationsList.Count,
                        OperationStartIndex = allOperations.Count,
                        ResultStartIndex = allResults.Count
                    };
                    
                    // Add operations to the response regardless of validity
                    allOperations.AddRange(operationsList.Select(OperationDto.FromDomain));
                    
                    if (invalidOperations.Any())
                    {
                        _logger.LogWarning("Invalid operations found in line: {Line}", line);
                        
                        // Add error results for each operation in this line
                        for (int i = 0; i < operationsList.Count; i++)
                        {
                            if (!operationsList[i].IsValid)
                            {
                                allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation: negative values not allowed"));
                            }
                            else
                            {
                                // Still process valid operations in the same line
                                allResults.Add(EnhancedTaxResultDto.FromDomain(new Domain.Models.TaxResult(0.0m)));
                            }
                        }
                        
                        // Complete scenario info
                        scenarioInfo.ResultCount = operationsList.Count;
                        scenarios.Add(scenarioInfo);
                        continue;
                    }

                    // Process operations (each line independently)
                    var results = await _processCapitalGainsUseCase.ExecuteAsync(operationsList, cancellationToken);
                    
                    // Add successful results
                    allResults.AddRange(results.Results.Select(EnhancedTaxResultDto.FromDomain));
                    
                    // Complete scenario info
                    scenarioInfo.ResultCount = results.Results.Count();
                    scenarios.Add(scenarioInfo);
                    
                    _logger.LogInformation("Successfully processed {OperationCount} operations from line #{LineIndex} with {ResultCount} results", 
                        operationsList.Count, lineIndex + 1, results.Results.Count());
                }
                catch (ArgumentException ex) when (ex.Message.Contains("Invalid operation sequence"))
                {
                    _logger.LogWarning(ex, "Invalid operation sequence in line: {Line}", line);
                    
                    // Try to parse operations anyway to show the error
                    try
                    {
                        var operations = _jsonSerializer.DeserializeOperations(line);
                        var operationsList = operations.ToList();
                        allOperations.AddRange(operationsList.Select(OperationDto.FromDomain));
                        for (int i = 0; i < operationsList.Count; i++)
                        {
                            allResults.Add(EnhancedTaxResultDto.FromError("Invalid operation sequence"));
                        }
                    }
                    catch
                    {
                        _logger.LogWarning("Could not parse operations from line with sequence error: {Line}", line);
                    }
                    continue;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Cannot sell more stocks than owned"))
                {
                    _logger.LogWarning(ex, "Invalid sell operation (insufficient stocks) in line: {Line}", line);
                    
                    // Try to parse operations anyway to show the error
                    try
                    {
                        var operations = _jsonSerializer.DeserializeOperations(line);
                        var operationsList = operations.ToList();
                        allOperations.AddRange(operationsList.Select(OperationDto.FromDomain));
                        for (int i = 0; i < operationsList.Count; i++)
                        {
                            allResults.Add(EnhancedTaxResultDto.FromError("Cannot sell more stocks than owned"));
                        }
                    }
                    catch
                    {
                        _logger.LogWarning("Could not parse operations from line with sell error: {Line}", line);
                    }
                    continue;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Invalid JSON in line: {Line}", line);
                    continue; // Skip invalid lines that can't be parsed
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error processing line: {Line}", line);
                    continue; // Skip any other errors
                }
            }

            // Convert results to response DTOs
            var response = new CalculationResultResponse
            {
                Operations = allOperations,
                Taxes = allResults,
                Scenarios = scenarios
            };

            _logger.LogInformation("Successfully processed file {FileName} with {LineCount} valid lines, generated {ResultCount} tax results",
                file.FileName, validJsonLines.Count, allResults.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file upload");
            return StatusCode(500, "Internal server error occurred while processing the file");
        }
    }

    /// <summary>
    /// Verifica se a API está funcionando corretamente
    /// </summary>
    /// <returns>Status da API e timestamp atual</returns>
    /// <response code="200">API está funcionando normalmente</response>
    /// <remarks>
    /// Endpoint para verificação de saúde da API. Retorna status "healthy" e timestamp atual.
    /// 
    /// Exemplo de resposta:
    /// ```
    /// {
    ///   "status": "healthy",
    ///   "timestamp": "2025-09-23T10:30:00.000Z"
    /// }
    /// ```
    /// </remarks>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult<object> HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}