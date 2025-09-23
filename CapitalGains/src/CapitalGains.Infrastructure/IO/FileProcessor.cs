using System.Text.Json;
using System.Text.RegularExpressions;

namespace CapitalGains.Infrastructure.IO;

/// <summary>
/// Interface for file processing services
/// </summary>
public interface IFileProcessor
{
    /// <summary>
    /// Processes a file content and extracts valid JSON lines
    /// </summary>
    IEnumerable<string> ProcessFileContent(string content);

    /// <summary>
    /// Validates if a line contains a valid JSON array
    /// </summary>
    bool IsValidJsonLine(string line);
}

/// <summary>
/// Service for processing files with mixed content (comments, invalid lines, etc.)
/// </summary>
public class FileProcessor : IFileProcessor
{
    private static readonly Regex JsonArrayPattern = new(@"^\s*\[.*\]\s*$", RegexOptions.Compiled);
    
    public IEnumerable<string> ProcessFileContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            yield break;

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || 
                trimmedLine.StartsWith("#") || 
                trimmedLine.StartsWith("//") ||
                trimmedLine.StartsWith("--") ||
                trimmedLine.StartsWith("==") ||
                trimmedLine.StartsWith("stdin:") ||
                trimmedLine.StartsWith("stdout:") ||
                trimmedLine.Contains("Scenario") ||
                trimmedLine.Contains("Case #") ||
                trimmedLine.Contains("End of file"))
            {
                continue;
            }

            // Check if line looks like a JSON array and is valid
            if (IsValidJsonLine(trimmedLine))
            {
                yield return trimmedLine;
            }
        }
    }

    public bool IsValidJsonLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var trimmedLine = line.Trim();

        // Must start with [ and end with ]
        if (!JsonArrayPattern.IsMatch(trimmedLine))
            return false;

        try
        {
            // Try to parse as JSON to validate structure
            using var document = JsonDocument.Parse(trimmedLine);
            
            // Must be an array
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return false;

            // Check if array contains valid operation objects
            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return false;

                // Must have required properties
                if (!element.TryGetProperty("operation", out _) ||
                    !element.TryGetProperty("unit-cost", out _) ||
                    !element.TryGetProperty("quantity", out _))
                {
                    return false;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}