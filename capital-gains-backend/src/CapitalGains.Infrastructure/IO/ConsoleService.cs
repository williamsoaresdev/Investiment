using Microsoft.Extensions.Logging;

namespace CapitalGains.Infrastructure.IO;

/// <summary>
/// Interface for console input/output operations
/// </summary>
public interface IConsoleService
{
    /// <summary>
    /// Reads all lines from standard input until empty line
    /// </summary>
    Task<IEnumerable<string>> ReadAllLinesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a line to standard output
    /// </summary>
    Task WriteLineAsync(string line, CancellationToken cancellationToken = default);
}

/// <summary>
/// Console service for handling standard input/output
/// </summary>
public class ConsoleService : IConsoleService
{
    private readonly ILogger<ConsoleService> _logger;

    public ConsoleService(ILogger<ConsoleService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<string>> ReadAllLinesAsync(CancellationToken cancellationToken = default)
    {
        var lines = new List<string>();
        
        try
        {
            _logger.LogDebug("Starting to read lines from standard input");
            
            string? line;
            while ((line = await Console.In.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
                // Stop reading when we encounter an empty line
                if (string.IsNullOrWhiteSpace(line))
                {
                    _logger.LogDebug("Empty line encountered, stopping input reading");
                    break;
                }
                
                lines.Add(line.Trim());
                _logger.LogDebug("Read line: {LineLength} characters", line.Length);
            }
            
            _logger.LogDebug("Finished reading {LineCount} lines from standard input", lines.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from standard input");
            throw;
        }

        return lines;
    }

    public async Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        try
        {
            await Console.Out.WriteLineAsync(line);
            _logger.LogDebug("Written line to standard output: {LineLength} characters", line.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to standard output");
            throw;
        }
    }
}