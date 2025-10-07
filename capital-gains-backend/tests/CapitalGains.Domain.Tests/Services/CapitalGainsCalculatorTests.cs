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
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 100),
            new Operation(OperationType.Sell, 15.00m, 50),
            new Operation(OperationType.Sell, 15.00m, 50)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(0.0m);
    }

    [Fact]
    public void ProcessOperations_Case2_ShouldReturnCorrectTaxes()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 20.00m, 5000),
            new Operation(OperationType.Sell, 5.00m, 5000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(10000.0m);
        results[2].Tax.Should().Be(0.0m);
    }

    [Fact]
    public void ProcessOperations_Case3_ShouldDeductAccumulatedLoss()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 5.00m, 5000),
            new Operation(OperationType.Sell, 20.00m, 3000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(1000.0m);
    }

    [Fact]
    public void ProcessOperations_Case4_ShouldHandleMultipleBuys()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Buy, 25.00m, 5000),
            new Operation(OperationType.Sell, 15.00m, 10000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(0.0m);
    }

    [Fact]
    public void ProcessOperations_Case5_ShouldCalculateCorrectWeightedAverage()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Buy, 25.00m, 5000),
            new Operation(OperationType.Sell, 15.00m, 10000),
            new Operation(OperationType.Sell, 25.00m, 5000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(4);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(0.0m);
        results[3].Tax.Should().Be(10000.0m);
    }

    [Fact]
    public void ProcessOperations_Case6_ShouldHandleSmallOperationsWithLoss()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 2.00m, 5000),
            new Operation(OperationType.Sell, 20.00m, 2000),
            new Operation(OperationType.Sell, 20.00m, 2000),
            new Operation(OperationType.Sell, 25.00m, 1000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(5);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(0.0m);
        results[3].Tax.Should().Be(0.0m);
        results[4].Tax.Should().Be(3000.0m);
    }

    [Fact]
    public void ProcessOperations_Case7_ShouldHandleNewBuyAfterAllSold()
    {
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

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(9);
        results[5].Tax.Should().Be(0.0m);
        results[6].Tax.Should().Be(0.0m);
        results[7].Tax.Should().Be(3700.0m);
        results[8].Tax.Should().Be(0.0m);
    }

    [Fact]
    public void ProcessOperations_Case8_ShouldHandleLargeProfits()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 50.00m, 10000),
            new Operation(OperationType.Buy, 20.00m, 10000),
            new Operation(OperationType.Sell, 50.00m, 10000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(4);
        results[0].Tax.Should().Be(0.0m);
        results[1].Tax.Should().Be(80000.0m);
        results[2].Tax.Should().Be(0.0m);
        results[3].Tax.Should().Be(60000.0m);
    }

    [Fact]
    public void ProcessOperations_WithInvalidOperation_ShouldThrowException()
    {
        var operations = new[]
        {
            new Operation(OperationType.Buy, 0, 100)
        };

        var action = () => _calculator.ProcessOperations(operations);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProcessOperations_EmptyList_ShouldReturnEmptyResults()
    {
        var operations = Array.Empty<Operation>();

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(0);
    }

    [Fact]
    public void ProcessOperations_SellMoreThanOwned_ShouldReturnError()
    {
        // Input #1: [{"operation":"buy", "unit-cost":10, "quantity": 10000}, {"operation":"sell", "unit-cost":20, "quantity": 11000}]
        // Output #1: [{"tax":0}, {"error":"Can't sell more stocks than you have"}]
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 20.00m, 11000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(2);
        results[0].Tax.Should().Be(0.0m);
        results[0].HasError.Should().BeFalse();
        results[1].HasError.Should().BeTrue();
        results[1].Error.Should().Be("Can't sell more stocks than you have");
        results[1].Tax.Should().Be(0.0m);
    }

    [Fact]
    public void ProcessOperations_SellMoreThanOwnedThenValidSell_ShouldContinueProcessing()
    {
        // Input #2: [{"operation":"buy", "unit-cost": 10, "quantity": 10000}, {"operation":"sell", "unit-cost":20, "quantity": 11000}, {"operation":"sell", "unit-cost": 20, "quantity": 5000}]
        // Output #2: [{"tax":0}, {"error":"Can't sell more stocks than you have"}, {"tax":10000}]
        var operations = new[]
        {
            new Operation(OperationType.Buy, 10.00m, 10000),
            new Operation(OperationType.Sell, 20.00m, 11000),
            new Operation(OperationType.Sell, 20.00m, 5000)
        };

        var results = _calculator.ProcessOperations(operations);

        results.Count.Should().Be(3);
        results[0].Tax.Should().Be(0.0m);
        results[0].HasError.Should().BeFalse();
        results[1].HasError.Should().BeTrue();
        results[1].Error.Should().Be("Can't sell more stocks than you have");
        results[1].Tax.Should().Be(0.0m);
        results[2].Tax.Should().Be(10000.0m);
        results[2].HasError.Should().BeFalse();
    }
}