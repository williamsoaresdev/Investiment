using CapitalGains.WebApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CapitalGains.WebApi.Tests;

/// <summary>
/// Integration tests for Capital Gains API
/// </summary>
public class CapitalGainsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CapitalGainsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/capitalgains/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task Calculate_WithValidOperations_ShouldReturnTaxResults()
    {
        // Arrange
        var request = new OperationsRequest
        {
            Operations = new[]
            {
                new CapitalGains.Infrastructure.Serialization.OperationDto 
                { 
                    Operation = "buy", 
                    UnitCost = 10.00m, 
                    Quantity = 100 
                },
                new CapitalGains.Infrastructure.Serialization.OperationDto 
                { 
                    Operation = "sell", 
                    UnitCost = 15.00m, 
                    Quantity = 50 
                }
            }
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/capitalgains/calculate", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationsResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        result.Should().NotBeNull();
        result!.Taxes.Should().HaveCount(2);
    }

    [Fact]
    public async Task Calculate_WithEmptyOperations_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new OperationsRequest { Operations = Array.Empty<CapitalGains.Infrastructure.Serialization.OperationDto>() };
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/capitalgains/calculate", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithValidJsonFile_ShouldReturnTaxResults()
    {
        // Arrange
        var jsonContent = @"[{""operation"":""buy"",""unit-cost"":10.00,""quantity"":100},{""operation"":""sell"",""unit-cost"":15.00,""quantity"":50}]";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "operations.json");

        // Act
        var response = await _client.PostAsync("/api/capitalgains/upload", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationsResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        result.Should().NotBeNull();
        result!.Taxes.Should().HaveCount(2);
    }

    [Fact]
    public async Task Upload_WithInvalidFileExtension_ShouldReturnBadRequest()
    {
        // Arrange
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test"));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "operations.xml");

        // Act
        var response = await _client.PostAsync("/api/capitalgains/upload", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithNoFile_ShouldReturnBadRequest()
    {
        // Arrange
        var formData = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/capitalgains/upload", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}