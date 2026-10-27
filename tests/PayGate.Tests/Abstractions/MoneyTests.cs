namespace PayGate.Tests.Abstractions;

using System.Globalization;

public sealed class MoneyTests
{
    private static readonly Currency _kwd = Currency.FromCode("KWD");
    private static readonly Currency _jpy = Currency.FromCode("JPY");

    [Fact]
    public void Rejects_a_negative_amount()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Money(-0.01m, Currency.Egp));
    }

    [Theory]
    [InlineData("10.001", "EGP")]
    [InlineData("1.5", "JPY")]
    [InlineData("1.0001", "KWD")]
    public void Rejects_more_decimals_than_the_currency_allows(string amount, string code)
    {
        Should.Throw<ArgumentException>(() => new Money(Parse(amount), Currency.FromCode(code)));
    }

    [Fact]
    public void Accepts_trailing_zeros_beyond_the_exponent()
    {
        new Money(10.100m, Currency.Egp).Amount.ShouldBe(10.1m);
    }

    [Fact]
    public void Rejects_a_default_currency()
    {
        Should.Throw<ArgumentException>(() => new Money(1m, default));
    }

    [Theory]
    [InlineData("10.005", "EGP", "10.01")]
    [InlineData("10.004", "EGP", "10.00")]
    [InlineData("2.5", "JPY", "3")]
    [InlineData("1.0005", "KWD", "1.001")]
    [InlineData("0.125", "USD", "0.13")]
    public void Round_rounds_midpoints_away_from_zero(string amount, string code, string expected)
    {
        Money.Round(Parse(amount), Currency.FromCode(code)).Amount.ShouldBe(Parse(expected));
    }

    [Fact]
    public void Round_honours_another_midpoint_mode()
    {
        Money.Round(2.5m, _jpy, MidpointRounding.ToEven).Amount.ShouldBe(2m);
    }

    [Theory]
    [InlineData("150.25", "EGP", 15025L)]
    [InlineData("0.01", "EGP", 1L)]
    [InlineData("0", "EGP", 0L)]
    [InlineData("1.234", "KWD", 1234L)]
    [InlineData("500", "JPY", 500L)]
    public void Converts_to_and_from_minor_units_exactly(string amount, string code, long minorUnits)
    {
        var currency = Currency.FromCode(code);
        var money = new Money(Parse(amount), currency);

        money.ToMinorUnits().ShouldBe(minorUnits);
        Money.FromMinorUnits(minorUnits, currency).ShouldBe(money);
    }

    [Fact]
    public void ToMinorUnits_throws_when_the_result_does_not_fit_in_a_long()
    {
        Should.Throw<OverflowException>(() => new Money(100_000_000_000_000_000m, Currency.Egp).ToMinorUnits());
    }

    [Fact]
    public void FromMinorUnits_rejects_a_negative_amount()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Money.FromMinorUnits(-1, Currency.Egp));
    }

    [Fact]
    public void Adds_and_subtracts_in_the_same_currency()
    {
        var a = new Money(10.50m, Currency.Egp);
        var b = new Money(0.25m, Currency.Egp);

        (a + b).ShouldBe(new Money(10.75m, Currency.Egp));
        (a - b).ShouldBe(new Money(10.25m, Currency.Egp));
        a.Subtract(a).IsZero.ShouldBeTrue();
    }

    [Fact]
    public void Subtract_never_goes_below_zero()
    {
        Should.Throw<InvalidOperationException>(() => new Money(1m, Currency.Egp) - new Money(1.01m, Currency.Egp));
    }

    [Fact]
    public void Combining_different_currencies_throws()
    {
        var egp = new Money(1m, Currency.Egp);
        var usd = new Money(1m, Currency.Usd);

        Should.Throw<InvalidOperationException>(() => egp + usd);
        Should.Throw<InvalidOperationException>(() => egp.CompareTo(usd));
    }

    [Fact]
    public void Compares_amounts_in_the_same_currency()
    {
        var small = new Money(1m, Currency.Egp);
        var large = new Money(2m, Currency.Egp);

        (small < large).ShouldBeTrue();
        (large > small).ShouldBeTrue();
        (small <= new Money(1.00m, Currency.Egp)).ShouldBeTrue();
        (large >= small).ShouldBeTrue();
    }

    [Fact]
    public void Equality_ignores_trailing_zeros()
    {
        new Money(10.10m, Currency.Egp).ShouldBe(new Money(10.1m, Currency.Egp));
    }

    [Fact]
    public void Zero_is_zero()
    {
        Money.Zero(Currency.Egp).IsZero.ShouldBeTrue();
    }

    [Theory]
    [InlineData("ar-EG")]
    [InlineData("de-DE")]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void ToString_is_invariant_under_any_culture(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

            new Money(1234.5m, Currency.Egp).ToString().ShouldBe("1234.50 EGP");
            new Money(1.25m, _kwd).ToString().ShouldBe("1.250 KWD");
            new Money(500m, _jpy).ToString().ShouldBe("500 JPY");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    private static decimal Parse(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);
}
