using CapitalGains.Application.Extensions;
using CapitalGains.Application.UseCases;
using CapitalGains.Infrastructure.Extensions;
using CapitalGains.Infrastructure.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CapitalGains.Integration.Tests;

/// <summary>
/// End-to-end integration tests covering all test cases from the specification
/// </summary>
public class CapitalGainsIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IProcessCapitalGainsUseCase _useCase;

    public CapitalGainsIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddApplicationServices();
        services.AddInfrastructureServices();
        
        _serviceProvider = services.BuildServiceProvider();
        _jsonSerializer = _serviceProvider.GetRequiredService<IJsonSerializer>();
        _useCase = _serviceProvider.GetRequiredService<IProcessCapitalGainsUseCase>();
    }

    [Fact]
    public async Task ProcessCapitalGains_Case1_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 100},
            {"operation":"sell", "unit-cost":15.00, "quantity": 50},
            {"operation":"sell", "unit-cost":15.00, "quantity": 50}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":0.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case2_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":20.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":5.00, "quantity": 5000}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":10000.0},{"tax":0.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case3_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":5.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":20.00, "quantity": 3000}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":1000.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case4_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"buy", "unit-cost":25.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":15.00, "quantity": 10000}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":0.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case5_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"buy", "unit-cost":25.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":15.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":25.00, "quantity": 5000}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":10000.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case6_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":2.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":20.00, "quantity": 2000},
            {"operation":"sell", "unit-cost":20.00, "quantity": 2000},
            {"operation":"sell", "unit-cost":25.00, "quantity": 1000}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":3000.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case7_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":2.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":20.00, "quantity": 2000},
            {"operation":"sell", "unit-cost":20.00, "quantity": 2000},
            {"operation":"sell", "unit-cost":25.00, "quantity": 1000},
            {"operation":"buy", "unit-cost":20.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":15.00, "quantity": 5000},
            {"operation":"sell", "unit-cost":30.00, "quantity": 4350},
            {"operation":"sell", "unit-cost":30.00, "quantity": 650}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":3000.0},{"tax":0.0},{"tax":0.0},{"tax":3700.0},{"tax":0.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case8_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation":"buy", "unit-cost":10.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":50.00, "quantity": 10000},
            {"operation":"buy", "unit-cost":20.00, "quantity": 10000},
            {"operation":"sell", "unit-cost":50.00, "quantity": 10000}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":80000.0},{"tax":0.0},{"tax":60000.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_Case9_ShouldReturnCorrectJson()
    {
        // Arrange
        var inputJson = """
            [{"operation": "buy", "unit-cost": 5000.00, "quantity": 10},
            {"operation": "sell", "unit-cost": 4000.00, "quantity": 5},
            {"operation": "buy", "unit-cost": 15000.00, "quantity": 5},
            {"operation": "buy", "unit-cost": 4000.00, "quantity": 2},
            {"operation": "buy", "unit-cost": 23000.00, "quantity": 2},
            {"operation": "sell", "unit-cost": 20000.00, "quantity": 1},
            {"operation": "sell", "unit-cost": 12000.00, "quantity": 10},
            {"operation": "sell", "unit-cost": 15000.00, "quantity": 3}]
            """;

        var expectedJson = """[{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":0.0},{"tax":1000.0},{"tax":2400.0}]""";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }

    [Fact]
    public async Task ProcessCapitalGains_MultipleLines_ShouldProcessIndependently()
    {
        // Arrange - Multiple lines should be processed independently
        var inputJson1 = """[{"operation":"buy", "unit-cost":10.00, "quantity": 100},{"operation":"sell", "unit-cost":15.00, "quantity": 50},{"operation":"sell", "unit-cost":15.00, "quantity": 50}]""";
        var inputJson2 = """[{"operation":"buy", "unit-cost":10.00, "quantity": 10000},{"operation":"sell", "unit-cost":20.00, "quantity": 5000},{"operation":"sell", "unit-cost":5.00, "quantity": 5000}]""";

        var expectedJson1 = """[{"tax":0.0},{"tax":0.0},{"tax":0.0}]""";
        var expectedJson2 = """[{"tax":0.0},{"tax":10000.0},{"tax":0.0}]""";

        // Act - Process first line
        var operations1 = _jsonSerializer.DeserializeOperations(inputJson1);
        var results1 = await _useCase.ExecuteAsync(operations1);
        var outputJson1 = _jsonSerializer.SerializeTaxResults(results1);

        // Act - Process second line (should be independent)
        var operations2 = _jsonSerializer.DeserializeOperations(inputJson2);
        var results2 = await _useCase.ExecuteAsync(operations2);
        var outputJson2 = _jsonSerializer.SerializeTaxResults(results2);

        // Assert
        outputJson1.Should().Be(expectedJson1);
        outputJson2.Should().Be(expectedJson2);
    }

    [Theory]
    [InlineData("invalid json")]
    [InlineData("[{\"operation\":\"invalid\", \"unit-cost\":10.00, \"quantity\": 100}]")]
    [InlineData("[{\"operation\":\"buy\", \"unit-cost\":-10.00, \"quantity\": 100}]")]
    public async Task ProcessCapitalGains_InvalidInput_ShouldThrowException(string invalidJson)
    {
        // Act & Assert
        var action = async () =>
        {
            var operations = _jsonSerializer.DeserializeOperations(invalidJson);
            await _useCase.ExecuteAsync(operations);
        };

        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProcessCapitalGains_EmptyInput_ShouldReturnEmptyResult()
    {
        // Arrange
        var inputJson = "[]";
        var expectedJson = "[]";

        // Act
        var operations = _jsonSerializer.DeserializeOperations(inputJson);
        var results = await _useCase.ExecuteAsync(operations);
        var outputJson = _jsonSerializer.SerializeTaxResults(results);

        // Assert
        outputJson.Should().Be(expectedJson);
    }
}