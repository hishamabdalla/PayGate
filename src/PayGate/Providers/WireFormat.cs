namespace PayGate.Providers;

using System.Globalization;
using System.Text.Json;

/// <summary>
/// Culture-proof formatting for provider requests and signatures. Under <c>ar-EG</c> a plain
/// <c>decimal.ToString("0.00")</c> uses <c>٫</c> as the decimal separator and breaks every signature, so all wire
/// values go through these helpers.
/// </summary>
public static class WireFormat
{
    private const int _maxDecimalPlaces = 28;

    /// <summary>Formats an amount with a fixed number of decimal places in invariant culture, for example <c>250.00</c>.</summary>
    /// <param name="amount">The amount.</param>
    /// <param name="decimalPlaces">The number of decimal places. Kashier and Fawry use 2.</param>
    /// <returns>The formatted amount.</returns>
    /// <exception cref="ArgumentException">The amount has more significant decimal places than <paramref name="decimalPlaces"/>. It is never rounded or truncated, because that would change a signed value.</exception>
    public static string ToDecimalString(decimal amount, int decimalPlaces = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(decimalPlaces, _maxDecimalPlaces);

        if (amount != Math.Round(amount, decimalPlaces))
        {
            throw new ArgumentException(
                $"{amount.ToString(CultureInfo.InvariantCulture)} has more than {decimalPlaces} decimal places.",
                nameof(amount));
        }

        return amount.ToString("F" + decimalPlaces.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }

    /// <summary>Formats money with its currency's decimal places in invariant culture: <c>150.00</c> for EGP, <c>1.250</c> for KWD.</summary>
    /// <param name="money">The money.</param>
    /// <returns>The formatted amount, without the currency code.</returns>
    public static string ToDecimalString(Money money) => ToDecimalString(money.Amount, money.Currency.MinorUnitExponent);

    /// <summary>Formats money as an integer count of minor units in invariant culture, for example <c>15000</c> (Paymob, Stripe).</summary>
    /// <param name="money">The money.</param>
    /// <returns>The minor units as a string.</returns>
    public static string ToMinorUnitsString(Money money) => money.ToMinorUnits().ToString(CultureInfo.InvariantCulture);

    /// <summary>Percent-encodes a value per RFC 3986: every character except <c>A–Z a–z 0–9 - . _ ~</c>, with uppercase hex.</summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>The encoded value.</returns>
    public static string PercentEncode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Uri.EscapeDataString(value);
    }

    /// <summary>
    /// Returns a JSON value exactly as the provider wrote it, so a signature is computed over the provider's text, not over a
    /// re-formatted number. Numbers keep their written form (<c>250.00</c> stays <c>250.00</c>), strings are unescaped,
    /// booleans are <c>true</c>/<c>false</c>, and objects and arrays are their raw JSON.
    /// </summary>
    /// <param name="element">The JSON value.</param>
    /// <returns>The raw text, or <see langword="null"/> for a JSON <c>null</c>.</returns>
    public static string? GetRawValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => element.GetRawText(),
    };

    /// <summary>Reads a property of a JSON object with <see cref="GetRawValue"/>. Property names match case-sensitively, as providers sign them.</summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The raw value, or <see langword="null"/> for a JSON <c>null</c>.</param>
    /// <returns><see langword="true"/> when the element is an object and has the property. A signer must fail closed when this is false.</returns>
    public static bool TryGetRawProperty(JsonElement element, string propertyName, out string? value)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        value = null;
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        value = GetRawValue(property);
        return true;
    }
}
