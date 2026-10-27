namespace PayGate.Tests.Abstractions;

public sealed class CurrencyTests
{
    [Theory]
    [InlineData("EGP", 2)]
    [InlineData("USD", 2)]
    [InlineData("KWD", 3)]
    [InlineData("BHD", 3)]
    [InlineData("JPY", 0)]
    [InlineData("ISK", 0)]
    public void FromCode_returns_the_iso_exponent(string code, int exponent)
    {
        var currency = Currency.FromCode(code);

        currency.Code.ShouldBe(code);
        currency.MinorUnitExponent.ShouldBe(exponent);
    }

    [Fact]
    public void FromCode_is_case_insensitive_and_trims()
    {
        Currency.FromCode(" egp ").ShouldBe(Currency.Egp);
    }

    [Fact]
    public void FromCode_rejects_a_currency_outside_the_table()
    {
        Should.Throw<ArgumentException>(() => Currency.FromCode("MAD"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("EG")]
    [InlineData("EGPP")]
    [InlineData("E1P")]
    [InlineData("MAD")]
    public void TryFromCode_returns_false_for_unknown_or_invalid_codes(string? code)
    {
        Currency.TryFromCode(code, out var currency).ShouldBeFalse();
        currency.ShouldBe(default);
    }

    [Fact]
    public void Create_accepts_a_currency_outside_the_table()
    {
        var currency = Currency.Create("mad", 2);

        currency.Code.ShouldBe("MAD");
        currency.MinorUnitExponent.ShouldBe(2);
    }

    [Fact]
    public void Create_accepts_a_known_currency_with_its_own_exponent()
    {
        Currency.Create("KWD", 3).ShouldBe(Currency.FromCode("KWD"));
    }

    [Fact]
    public void Create_rejects_an_exponent_that_contradicts_the_table()
    {
        Should.Throw<ArgumentException>(() => Currency.Create("EGP", 3));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void Create_rejects_an_exponent_out_of_range(int exponent)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Currency.Create("MAD", exponent));
    }

    [Theory]
    [InlineData("MA")]
    [InlineData("MADD")]
    [InlineData("M4D")]
    public void Create_rejects_a_code_that_is_not_three_letters(string code)
    {
        Should.Throw<ArgumentException>(() => Currency.Create(code, 2));
    }

    [Fact]
    public void ToString_is_the_code()
    {
        Currency.Egp.ToString().ShouldBe("EGP");
    }

    [Fact]
    public void Default_has_an_empty_code()
    {
        default(Currency).Code.ShouldBe(string.Empty);
    }
}
