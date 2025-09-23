using CapitalGains.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CapitalGains.Domain.Tests.Models;

public class PortfolioTests
{
    [Fact]
    public void Portfolio_NewInstance_ShouldBeEmpty()
    {
        // Arrange & Act
        var portfolio = new Portfolio();

        // Assert
        portfolio.AveragePrice.Should().Be(0m);
        portfolio.Quantity.Should().Be(0);
        portfolio.AccumulatedLoss.Should().Be(0m);
        portfolio.HasStocks.Should().BeFalse();
    }

    [Fact]
    public void Buy_FirstPurchase_ShouldSetAveragePrice()
    {
        // Arrange
        var portfolio = new Portfolio();

        // Act
        portfolio.Buy(10.00m, 100);

        // Assert
        portfolio.AveragePrice.Should().Be(10.00m);
        portfolio.Quantity.Should().Be(100);
        portfolio.HasStocks.Should().BeTrue();
    }

    [Fact]
    public void Buy_MultiplePurchases_ShouldCalculateWeightedAverage()
    {
        // Arrange
        var portfolio = new Portfolio();

        // Act
        portfolio.Buy(10.00m, 10000);
        portfolio.Buy(25.00m, 5000);

        // Assert
        // Expected: ((10 * 10000) + (25 * 5000)) / 15000 = 15.00
        portfolio.AveragePrice.Should().Be(15.00m);
        portfolio.Quantity.Should().Be(15000);
    }

    [Fact]
    public void Sell_WithProfit_ShouldCalculateTax()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 10000);

        // Act
        var result = portfolio.Sell(20.00m, 5000);

        // Assert
        // Profit: (20 - 10) * 5000 = 50000
        // Tax: 50000 * 0.20 = 10000
        result.Tax.Should().Be(10000.00m);
        portfolio.Quantity.Should().Be(5000);
        portfolio.AveragePrice.Should().Be(10.00m); // Unchanged
    }

    [Fact]
    public void Sell_WithLoss_ShouldNotCalculateTaxAndAccumulateLoss()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 10000);

        // Act
        var result = portfolio.Sell(5.00m, 5000);

        // Assert
        // Loss: (5 - 10) * 5000 = -25000
        result.Tax.Should().Be(0m);
        portfolio.AccumulatedLoss.Should().Be(25000m);
        portfolio.Quantity.Should().Be(5000);
    }

    [Fact]
    public void Sell_SmallOperation_ShouldNotCalculateTax()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);

        // Act
        var result = portfolio.Sell(15.00m, 50);

        // Assert
        // Total operation: 15 * 50 = 750 (< 20000)
        result.Tax.Should().Be(0m);
    }

    [Fact]
    public void Sell_SmallOperationWithLoss_ShouldAccumulateLoss()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 1000);

        // Act
        var result = portfolio.Sell(8.00m, 500);

        // Assert
        // Total operation: 8 * 500 = 4000 (< 20000)
        // Loss: (8 - 10) * 500 = -1000
        result.Tax.Should().Be(0m);
        portfolio.AccumulatedLoss.Should().Be(1000m);
    }

    [Fact]
    public void Sell_WithAccumulatedLoss_ShouldDeductFromProfit()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 10000);
        
        // Create accumulated loss
        portfolio.Sell(5.00m, 5000); // Loss of 25000
        
        // Act
        var result = portfolio.Sell(20.00m, 3000);

        // Assert
        // Profit: (20 - 10) * 3000 = 30000
        // After deducting loss: 30000 - 25000 = 5000
        // Tax: 5000 * 0.20 = 1000
        result.Tax.Should().Be(1000.00m);
        portfolio.AccumulatedLoss.Should().Be(0m);
    }

    [Fact]
    public void Sell_AllStocks_ShouldResetAveragePrice()
    {
        // Arrange
        var portfolio = new Portfolio();
        portfolio.Buy(10.00m, 100);

        // Act
        portfolio.Sell(15.00m, 100);

        // Assert
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