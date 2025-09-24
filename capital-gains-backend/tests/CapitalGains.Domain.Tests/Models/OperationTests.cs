using CapitalGains.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CapitalGains.Domain.Tests.Models;

public class OperationTests
{
    [Fact]
    public void Operation_WithValidBuyData_ShouldCreateCorrectly()
    {
        var operation = new Operation(OperationType.Buy, 10.00m, 100);

        operation.Type.Should().Be(OperationType.Buy);
        operation.UnitCost.Should().Be(10.00m);
        operation.Quantity.Should().Be(100);
        operation.TotalValue.Should().Be(1000.00m);
        operation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Operation_WithValidSellData_ShouldCreateCorrectly()
    {
        var operation = new Operation(OperationType.Sell, 15.50m, 50);

        operation.Type.Should().Be(OperationType.Sell);
        operation.UnitCost.Should().Be(15.50m);
        operation.Quantity.Should().Be(50);
        operation.TotalValue.Should().Be(775.00m);
        operation.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    [InlineData(0, 0)]
    public void Operation_WithInvalidData_ShouldBeInvalid(decimal unitCost, int quantity)
    {
        var operation = new Operation(OperationType.Buy, unitCost, quantity);

        operation.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TotalValue_ShouldCalculateCorrectly()
    {
        var operation = new Operation(OperationType.Buy, 12.34m, 567);

        operation.TotalValue.Should().Be(6996.78m);
    }
}