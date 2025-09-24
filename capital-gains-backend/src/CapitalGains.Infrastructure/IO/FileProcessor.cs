using System.Text.Json;
using System.Text.RegularExpressions;

namespace CapitalGains.Infrastructure.IO;

public interface IFileProcessor
{
    IEnumerable<string> ProcessFileContent(string content);

    bool IsValidJsonLine(string line);
}

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

        if (!JsonArrayPattern.IsMatch(trimmedLine))
            return false;

        try
        {
            using var document = JsonDocument.Parse(trimmedLine);
            
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return false;

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                    return false;

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