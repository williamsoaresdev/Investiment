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
        
        Console.WriteLine($"[FileProcessor] Processing {lines.Length} lines from file");

        foreach (var (line, index) in lines.Select((line, index) => (line, index)))
        {
            var trimmedLine = line.Trim();
            
            Console.WriteLine($"[FileProcessor] Line {index + 1}: '{trimmedLine.Substring(0, Math.Min(50, trimmedLine.Length))}...'");
            
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
                Console.WriteLine($"[FileProcessor] Skipping line {index + 1} (comment/metadata)");
                continue;
            }

            // Check if line looks like a JSON array and is valid
            if (IsValidJsonLine(trimmedLine))
            {
                Console.WriteLine($"[FileProcessor] Found valid JSON line {index + 1}");
                yield return trimmedLine;
            }
            else
            {
                Console.WriteLine($"[FileProcessor] Line {index + 1} is not valid JSON");
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