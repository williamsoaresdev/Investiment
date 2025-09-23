using CapitalGains.Domain.Models;
using CapitalGains.Domain.Services;
using FluentAssertions;
using Xunit;

namespace CapitalGains.Domain.Tests.Services;

public class CapitalGainsCalculatorTests
{
    private readonly CapitalGainsCalculator _calculator = new();

    [Fact]
    public void ProcessOperations_Case1_ShouldReturnCorrectTaxes()
    {
        // Arrange - Case #1: Small operations, no tax
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 100),
            new Operation(OperationType.Sell, 15.00m, 50),
            new Operation(OperationType.Sell, 15.00m, 50)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(0.0m);
    }

    [Fact]
    public void ProcessOperations_Case2_ShouldReturnCorrectTaxes()
    {
        // Arrange - Case #2: Large operations with profit and loss
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 20.00m, 5000),
            new Operation(OperationType.Sell, 5.00m, 5000)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);     // Buy operation
        results[1].Tax.Should().Be(10000.0m); // Profit of 50000, tax 20%
        results[2].Tax.Should().Be(0.0m);     // Loss operation
    }

    [Fact]
    public void ProcessOperations_Case3_ShouldDeductAccumulatedLoss()
    {
        // Arrange - Case #3: Loss deduction from future profit
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 5.00m, 5000),
            new Operation(OperationType.Sell, 20.00m, 3000)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);    // Buy operation
        results[1].Tax.Should().Be(0.0m);    // Loss of 25000
        results[2].Tax.Should().Be(1000.0m); // Profit 30000 - loss 25000 = 5000, tax 20%
    }

    [Fact]
    public void ProcessOperations_Case4_ShouldHandleMultipleBuys()
    {
        // Arrange - Case #4: Multiple buys with weighted average
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Buy, 25.00m, 5000),
            new Operation(OperationType.Sell, 15.00m, 10000)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m); // Buy operation
        results[1].Tax.Should().Be(0.0m); // Buy operation
        results[2].Tax.Should().Be(0.0m); // No profit (15 = weighted average)
    }

    [Fact]
    public void ProcessOperations_Case5_ShouldCalculateCorrectWeightedAverage()
    {
        // Arrange - Case #5: Weighted average calculation
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Buy, 25.00m, 5000),
            new Operation(OperationType.Sell, 15.00m, 10000),
            new Operation(OperationType.Sell, 25.00m, 5000)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(4);
        results[0].Tax.Should().Be(0.0m);     // Buy operation
        results[1].Tax.Should().Be(0.0m);     // Buy operation
        results[2].Tax.Should().Be(0.0m);     // Break even
        results[3].Tax.Should().Be(10000.0m); // Profit: (25-15)*5000 = 50000, tax 20%
    }

    [Fact]
    public void ProcessOperations_Case6_ShouldHandleSmallOperationsWithLoss()
    {
        // Arrange - Case #6: Small operations with accumulated loss
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 2.00m, 5000),
            new Operation(OperationType.Sell, 20.00m, 2000),
            new Operation(OperationType.Sell, 20.00m, 2000),
            new Operation(OperationType.Sell, 25.00m, 1000)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(5);
        results[0].Tax.Should().Be(0.0m);    // Buy operation
        results[1].Tax.Should().Be(0.0m);    // Loss 40000, but operation < 20000
        results[2].Tax.Should().Be(0.0m);    // Profit 20000, but after loss deduction = 0
        results[3].Tax.Should().Be(0.0m);    // Profit 20000, but after loss deduction = 0
        results[4].Tax.Should().Be(3000.0m); // Profit 15000, no more accumulated loss
    }

    [Fact]
    public void ProcessOperations_Case7_ShouldHandleNewBuyAfterAllSold()
    {
        // Arrange - Case #7: New purchases after selling all stocks
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 2.00m, 5000),
            new Operation(OperationType.Sell, 20.00m, 2000),
            new Operation(OperationType.Sell, 20.00m, 2000),
            new Operation(OperationType.Sell, 25.00m, 1000),
            new Operation(OperationType.Buy, 20.00m, 10000),
            new Operation(OperationType.Sell, 15.00m, 5000),
            new Operation(OperationType.Sell, 30.00m, 4350),
            new Operation(OperationType.Sell, 30.00m, 650)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(9);
        results[5].Tax.Should().Be(0.0m);    // Buy operation
        results[6].Tax.Should().Be(0.0m);    // Loss of 25000
        results[7].Tax.Should().Be(3700.0m); // Profit 43500 - loss 25000 = 18500, tax 20%
        results[8].Tax.Should().Be(0.0m);    // Small operation (< 20000)
    }

    [Fact]
    public void ProcessOperations_Case8_ShouldHandleLargeProfits()
    {
        // Arrange - Case #8: Large profits
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 50.00m, 10000),
            new Operation(OperationType.Buy, 20.00m, 10000),
            new Operation(OperationType.Sell, 50.00m, 10000)
        };

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(4);
        results[0].Tax.Should().Be(0.0m);     // Buy operation
        results[1].Tax.Should().Be(80000.0m); // Profit 400000, tax 20%
        results[2].Tax.Should().Be(0.0m);     // Buy operation
        results[3].Tax.Should().Be(60000.0m); // Profit 300000, tax 20%
    }

    [Fact]
    public void ProcessOperations_WithInvalidOperation_ShouldThrowException()
    {
        // Arrange
        var operations = new[]
        {
            new Operation(OperationType.Buy, 0, 100) // Invalid operation
        };

        // Act & Assert
        var action = () => _calculator.ProcessOperations(operations);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProcessOperations_EmptyList_ShouldReturnEmptyResults()
    {
        // Arrange
        var operations = Array.Empty<Operation>();

        // Act
        var results = _calculator.ProcessOperations(operations);

        // Assert
        results.Count.Should().Be(0);
    }
}