using PayFlow.SharedKernel;

namespace PayFlow.Modules.Payments.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_ValidInput_NormalisesCurrency()
    {
        var result = Money.Create(10.50m, " eur ");

        Assert.True(result.IsSuccess);
        Assert.Equal("EUR", result.Value.Currency);
        Assert.Equal(10.50m, result.Value.Amount);
    }

    [Fact]
    public void Create_NegativeAmount_Fails()
    {
        var result = Money.Create(-1m, "EUR");
        Assert.Equal(MoneyErrors.NegativeAmount, result.Error);
    }

    [Fact]
    public void Create_ThreeDecimals_Fails()
    {
        var result = Money.Create(1.999m, "EUR");
        Assert.Equal(MoneyErrors.TooManyDecimals, result.Error);
    }

    [Fact]
    public void Create_UnsupportedCurrency_Fails()
    {
        var result = Money.Create(10m, "XYZ");
        Assert.Equal("Money.UnsupportedCurrency", result.Error.Code);
    }

    [Fact]
    public void SameAmountAndCurrency_AreEqual()
    {
        Assert.Equal(Money.Create(5m, "EUR").Value, Money.Create(5m, "EUR").Value);
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        var eur = Money.Create(5m, "EUR").Value;
        var usd = Money.Create(5m, "USD").Value;

        Assert.Throws<InvalidOperationException>(() => eur.Add(usd));
    }
}