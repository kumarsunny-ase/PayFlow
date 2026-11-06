namespace PayFlow.SharedKernel;

public sealed record Money
{
    public static readonly HashSet<string> SupportedCurrencies = [
    "USD",
    "EUR",
    "GBP",
    "CAD",
    "AUD",
    "CHF"
];
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string currency)
    {
        var code = (currency?? string.Empty).Trim().ToUpperInvariant();

        if(!SupportedCurrencies.Contains(code))
            return MoneyErrors.UnsupportedCurrency(code);

        if(amount < 0)
            return MoneyErrors.NegativeAmount;

        if(decimal.Round(amount, 2) != amount)
            return MoneyErrors.TooManyDecimals;

        return new Money(amount, code);
    }

    public static Money Zero(string currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        if (other.Amount > Amount)
            throw new InvalidOperationException("Result would be negative.");
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}.");
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";

}

public static class MoneyErrors
{
    public static readonly Error NegativeAmount =
        new("Money.NegativeAmount", "Amount cannot be negative.");

    public static readonly Error TooManyDecimals =
        new("Money.TooManyDecimals", "Amount can have at most 2 decimal places.");

    public static Error UnsupportedCurrency(string currency) =>
        new("Money.UnsupportedCurrency", $"Currency '{currency}' is not supported.");
}

