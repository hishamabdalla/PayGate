namespace PayGate.Tests.Core;

using System.Globalization;
using System.Text.Json;
using PayGate.Providers;

public sealed class WireFormatTests
{
    [Theory]
    [InlineData("ar-EG")]
    [InlineData("de-DE")]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void Decimal_strings_are_invariant_under_any_culture(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

            WireFormat.ToDecimalString(250m).ShouldBe("250.00");
            WireFormat.ToDecimalString(1234.5m).ShouldBe("1234.50");
            WireFormat.ToDecimalString(1234.5m, 3).ShouldBe("1234.500");
            WireFormat.ToDecimalString(7m, 0).ShouldBe("7");
            WireFormat.ToDecimalString(new Money(150m, Currency.Egp)).ShouldBe("150.00");
            WireFormat.ToDecimalString(new Money(1.25m, Currency.FromCode("KWD"))).ShouldBe("1.250");
            WireFormat.ToMinorUnitsString(new Money(1234567.89m, Currency.Egp)).ShouldBe("123456789");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void ToDecimalString_keeps_trailing_zeros_beyond_the_places()
    {
        WireFormat.ToDecimalString(250.000m).ShouldBe("250.00");
    }

    [Fact]
    public void ToDecimalString_never_rounds_a_signed_value()
    {
        Should.Throw<ArgumentException>(() => WireFormat.ToDecimalString(0.125m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(29)]
    public void ToDecimalString_rejects_invalid_decimal_places(int places)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => WireFormat.ToDecimalString(1m, places));
    }

    [Theory]
    [InlineData("abc-._~", "abc-._~")]
    [InlineData("a b", "a%20b")]
    [InlineData("a+b=c&d", "a%2Bb%3Dc%26d")]
    [InlineData("https://x.io/r?a=1", "https%3A%2F%2Fx.io%2Fr%3Fa%3D1")]
    [InlineData("ü", "%C3%BC")]
    [InlineData("!*'()", "%21%2A%27%28%29")]
    public void PercentEncode_follows_rfc_3986(string value, string expected)
    {
        WireFormat.PercentEncode(value).ShouldBe(expected);
    }

    [Fact]
    public void Raw_values_are_returned_as_the_provider_wrote_them()
    {
        using var json = JsonDocument.Parse(
            """{"amount":250.00,"minor":11334,"exp":1E2,"text":"a&b","flag":true,"none":null,"obj":{"k":1},"list":[1, 2]}""");
        var root = json.RootElement;

        Raw(root, "amount").ShouldBe("250.00");
        Raw(root, "minor").ShouldBe("11334");
        Raw(root, "exp").ShouldBe("1E2");
        Raw(root, "text").ShouldBe("a&b");
        Raw(root, "flag").ShouldBe("true");
        Raw(root, "none").ShouldBeNull();
        Raw(root, "obj").ShouldBe("""{"k":1}""");
        Raw(root, "list").ShouldBe("[1, 2]");
    }

    [Fact]
    public void TryGetRawProperty_reports_a_missing_property_so_signers_can_fail_closed()
    {
        using var json = JsonDocument.Parse("""{"Amount":1,"none":null}""");

        WireFormat.TryGetRawProperty(json.RootElement, "amount", out string? missing).ShouldBeFalse();
        missing.ShouldBeNull();
        WireFormat.TryGetRawProperty(json.RootElement, "none", out string? none).ShouldBeTrue();
        none.ShouldBeNull();
    }

    [Fact]
    public void TryGetRawProperty_returns_false_for_a_non_object()
    {
        using var json = JsonDocument.Parse("[1]");

        WireFormat.TryGetRawProperty(json.RootElement, "amount", out _).ShouldBeFalse();
    }

    private static string? Raw(JsonElement root, string name) =>
        WireFormat.TryGetRawProperty(root, name, out string? value) ? value : throw new InvalidOperationException(name);
}
