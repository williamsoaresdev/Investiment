using CapitalGains.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CapitalGains.Domain.Tests.Models;

public class PortfolioTests
{
    [Fact]
    public void Portfolio_NewInstance_ShouldBeEmpty()
    {
        var portfolio = new Portfolio();

        portfolio.AveragePrice.Should().Be(0m);
        portfolio.Quantity.Should().Be(0);
        portfolio.AccumulatedLoss.Should().Be(0m);
        portfolio.HasStocks.Should().BeFalse();
    }

    [Fact]
    public void Buy_FirstPurchase_ShouldSetAveragePrice()
    {
        var portfolio = new Portfolio();

        portfolio.Buy(10.00m, 100);

        portfolio.AveragePrice.Should().Be(10.00m);
        portfolio.Quantity.Should().Be(100);
        portfolio.HasStocks.Should().BeTrue();
    }

    [Fact]
    public void Buy_MultiplePurchases_ShouldCalculateWeightedAverage()
    {
        var portfolio = new Portfolio();

        portfolio.Buy(10.00m, 10000);
        portfolio.Buy(25.00m, 5000);

        portfolio.AveragePrice.Should().Be(15.00m);
        portfolio.Quantity.Should().Be(15000);
    }

    [Fact]
    public void Sell_WithProfit_ShouldCalculateTax()
    {
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 10000);

        var result = portfolio.Sell(20.00m, 5000);

        result.Tax.Should().Be(10000.00m);
        portfolio.Quantity.Should().Be(5000);
        portfolio.AveragePrice.Should().Be(10.00m);
    }

    [Fact]
    public void Sell_WithLoss_ShouldNotCalculateTaxAndAccumulateLoss()
    {
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 10000);

        var result = portfolio.Sell(5.00m, 5000);

        result.Tax.Should().Be(0m);
        portfolio.AccumulatedLoss.Should().Be(25000m);
        portfolio.Quantity.Should().Be(5000);
    }

    [Fact]
    public void Sell_SmallOperation_ShouldNotCalculateTax()
    {
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);

        var result = portfolio.Sell(15.00m, 50);

        result.Tax.Should().Be(0m);
    }

    [Fact]
    public void Sell_SmallOperationWithLoss_ShouldAccumulateLoss()
    {
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 1000);

        var result = portfolio.Sell(8.00m, 500);

        result.Tax.Should().Be(0m);
        portfolio.AccumulatedLoss.Should().Be(1000m);
    }

    [Fact]
    public void Sell_WithAccumulatedLoss_ShouldDeductFromProfit()
    {
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 10000);
        
        portfolio.Sell(5.00m, 5000);
        
        var result = portfolio.Sell(20.00m, 3000);

        result.Tax.Should().Be(1000.00m);
        portfolio.AccumulatedLoss.Should().Be(0m);
    }

    [Fact]
    public void Sell_AllStocks_ShouldResetAveragePrice()
    {
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);

        portfolio.Sell(15.00m, 100);

        portfolio.Quantity.Should().Be(0);
        portfolio.AveragePrice.Should().Be(0m);
        portfolio.HasStocks.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(0, 100)]
    [InlineData(10, -1)]
    [InlineData(10, 0)]
    public void Buy_WithInvalidParameters_ShouldThrowException(decimal unitCost, int quantity)
    {
        // Arrange
        var portfolio = new Portfolio();

        // Act & Assert
        var action = () => portfolio.Buy(unitCost, quantity);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(0, 100)]
    [InlineData(10, -1)]
    [InlineData(10, 0)]
    public void Sell_WithInvalidParameters_ShouldThrowException(decimal unitCost, int quantity)
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);

        // Act & Assert
        var action = () => portfolio.Sell(unitCost, quantity);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Sell_MoreThanOwned_ShouldThrowException()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);

        // Act & Assert
        var action = () => portfolio.Sell(15.00m, 101);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reset_ShouldClearAllState()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);
        portfolio.Sell(5.00m, 50); // Creates loss

        // Act
        portfolio.Reset();

        // Assert
        portfolio.AveragePrice.Should().Be(0m);
        portfolio.Quantity.Should().Be(0);
        portfolio.AccumulatedLoss.Should().Be(0m);
        portfolio.HasStocks.Should().BeFalse();
    }
}