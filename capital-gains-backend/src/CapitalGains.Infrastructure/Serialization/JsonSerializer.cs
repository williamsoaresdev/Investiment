using System.Text.Json;
using System.Text.Encodings.Web;
using CapitalGains.Domain.Models;
using CapitalGains.Infrastructure.Serialization;

namespace CapitalGains.Infrastructure.Serialization;

public interface IJsonSerializer
{
    IEnumerable<Operation> DeserializeOperations(string json);

    string SerializeTaxResults(TaxResultCollection results);
}

public class JsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
        WriteIndented = false,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public IEnumerable<Operation> DeserializeOperations(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Enumerable.Empty<Operation>();

        try
        {
            var operationDtos = System.Text.Json.JsonSerializer.Deserialize<OperationDto[]>(json, JsonOptions);
            
            if (operationDtos == null)
                return Enumerable.Empty<Operation>();

            return operationDtos.Select(dto => dto.ToDomain()).ToList();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", ex);
        }
    }

    public string SerializeTaxResults(TaxResultCollection results)
    {
        var taxResultDtos = results.Results
            .Select(TaxResultDto.FromDomain)
            .ToArray();

        var json = System.Text.Json.JsonSerializer.Serialize(taxResultDtos, JsonOptions);
        
        json = json.Replace("\"tax\":0", "\"tax\":0.0");
        json = System.Text.RegularExpressions.Regex.Replace(json, @"""tax"":(\d+)\.00", @"""tax"":$1.0");
        
        return json;
    }
}