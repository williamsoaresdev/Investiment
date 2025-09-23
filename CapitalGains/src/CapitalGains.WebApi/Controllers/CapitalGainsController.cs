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
    [ProducesResponseType(typeof(OperationsResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<OperationsResponse>> CalculateCapitalGains(
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

            // Convert DTOs to domain models
            var operations = request.Operations.Select(dto => dto.ToDomain()).ToList();

            // Validate operations
            var invalidOperations = operations.Where(op => !op.IsValid).ToList();
            if (invalidOperations.Any())
            {
                return BadRequest("All operations must have positive unit cost and quantity");
            }

            // Process operations
            var results = await _processCapitalGainsUseCase.ExecuteAsync(operations, cancellationToken);

            // Convert results to response DTOs
            var response = new OperationsResponse
            {
                Taxes = results.Results.Select(TaxResultDto.FromDomain)
            };

            _logger.LogInformation("Successfully processed {OperationCount} operations, generated {ResultCount} tax results",
                operations.Count, results.Count);

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
    [ProducesResponseType(typeof(OperationsResponse), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<OperationsResponse>> UploadFile(
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

            // Process file content
            var allResults = new List<Domain.Models.TaxResult>();

            foreach (var line in validJsonLines)
            {
                try
                {
                    // Parse operations from JSON line
                    var operations = _jsonSerializer.DeserializeOperations(line);
                    
                    // Validate operations
                    var operationsList = operations.ToList();
                    var invalidOperations = operationsList.Where(op => !op.IsValid).ToList();
                    if (invalidOperations.Any())
                    {
                        _logger.LogWarning("Invalid operations found in line: {Line}", line);
                        continue; // Skip invalid operations instead of failing
                    }

                    // Process operations
                    var results = await _processCapitalGainsUseCase.ExecuteAsync(operationsList, cancellationToken);
                    allResults.AddRange(results.Results);
                }
                catch (ArgumentException ex) when (ex.Message.Contains("Invalid operation sequence"))
                {
                    _logger.LogWarning(ex, "Skipping invalid operation sequence in line: {Line}", line);
                    continue; // Skip lines with invalid operation sequences
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Skipping invalid JSON line: {Line}", line);
                    continue; // Skip invalid lines instead of failing
                }
            }

            // Convert results to response DTOs
            var response = new OperationsResponse
            {
                Taxes = allResults.Select(TaxResultDto.FromDomain)
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