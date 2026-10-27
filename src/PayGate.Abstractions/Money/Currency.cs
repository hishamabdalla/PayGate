namespace PayGate;

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// An ISO 4217 currency and its minor-unit exponent (2 for EGP, 3 for KWD, 0 for JPY).
/// </summary>
public readonly record struct Currency
{
    private const int _maxExponent = 4;

    private static readonly FrozenDictionary<string, int> _knownExponents = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["EGP"] = 2,
        ["USD"] = 2,
        ["EUR"] = 2,
        ["GBP"] = 2,
        ["SAR"] = 2,
        ["AED"] = 2,
        ["QAR"] = 2,
        ["KWD"] = 3,
        ["BHD"] = 3,
        ["OMR"] = 3,
        ["JOD"] = 3,
        ["TND"] = 3,
        ["LYD"] = 3,
        ["IQD"] = 3,
        ["JPY"] = 0,
        ["KRW"] = 0,
        ["VND"] = 0,
        ["CLP"] = 0,
        ["ISK"] = 0,
        ["UGX"] = 0,
        ["XOF"] = 0,
        ["XAF"] = 0,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private Currency(string code, int minorUnitExponent)
    {
        Code = code;
        MinorUnitExponent = minorUnitExponent;
    }

    /// <summary>The three-letter uppercase ISO 4217 code. Empty for <see langword="default"/>.</summary>
    public string Code { get => field ?? string.Empty; private init; }

    /// <summary>The number of decimal places of the currency's minor unit.</summary>
    public int MinorUnitExponent { get; }

    /// <summary>Egyptian pound.</summary>
    public static Currency Egp { get; } = FromCode("EGP");

    /// <summary>US dollar.</summary>
    public static Currency Usd { get; } = FromCode("USD");

    /// <summary>Euro.</summary>
    public static Currency Eur { get; } = FromCode("EUR");

    /// <summary>Pound sterling.</summary>
    public static Currency Gbp { get; } = FromCode("GBP");

    /// <summary>Saudi riyal.</summary>
    public static Currency Sar { get; } = FromCode("SAR");

    /// <summary>UAE dirham.</summary>
    public static Currency Aed { get; } = FromCode("AED");

    /// <summary>Returns a currency from the built-in table.</summary>
    /// <param name="code">The ISO 4217 code. Case-insensitive.</param>
    /// <returns>The currency.</returns>
    /// <exception cref="ArgumentException">The code is not in the built-in table. Use <see cref="Create"/> for other currencies.</exception>
    public static Currency FromCode(string code) =>
        TryFromCode(code, out var currency)
            ? currency
            : throw new ArgumentException($"'{code}' is not a known currency. Use Currency.Create(code, exponent) for currencies outside the built-in table.", nameof(code));

    /// <summary>Tries to return a currency from the built-in table.</summary>
    /// <param name="code">The ISO 4217 code. Case-insensitive.</param>
    /// <param name="currency">The currency, when found.</param>
    /// <returns><see langword="true"/> when the code is in the built-in table.</returns>
    public static bool TryFromCode([NotNullWhen(true)] string? code, out Currency currency)
    {
        currency = default;
        if (code is null || !TryNormalizeCode(code, out string? normalized) || !_knownExponents.TryGetValue(normalized, out int exponent))
        {
            return false;
        }

        currency = new Currency(normalized, exponent);
        return true;
    }

    /// <summary>Creates a currency that is not in the built-in table.</summary>
    /// <param name="code">The three-letter ISO 4217 code. Case-insensitive.</param>
    /// <param name="minorUnitExponent">The minor-unit exponent, 0 to 4.</param>
    /// <returns>The currency.</returns>
    /// <exception cref="ArgumentException">The code is not three letters, or it is in the built-in table with a different exponent.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The exponent is outside 0 to 4.</exception>
    public static Currency Create(string code, int minorUnitExponent)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentOutOfRangeException.ThrowIfNegative(minorUnitExponent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minorUnitExponent, _maxExponent);

        if (!TryNormalizeCode(code, out string? normalized))
        {
            throw new ArgumentException($"'{code}' is not a three-letter ISO 4217 code.", nameof(code));
        }

        if (_knownExponents.TryGetValue(normalized, out int known) && known != minorUnitExponent)
        {
            throw new ArgumentException($"{normalized} has a minor-unit exponent of {known}, not {minorUnitExponent}.", nameof(minorUnitExponent));
        }

        return new Currency(normalized, minorUnitExponent);
    }

    /// <summary>Returns the ISO 4217 code.</summary>
    /// <returns>The code, for example <c>EGP</c>.</returns>
    public override string ToString() => Code;

    internal decimal MinorUnitFactor => MinorUnitExponent switch
    {
        0 => 1m,
        1 => 10m,
        2 => 100m,
        3 => 1000m,
        _ => 10000m,
    };

    private static bool TryNormalizeCode(string code, [NotNullWhen(true)] out string? normalized)
    {
        normalized = null;
        string trimmed = code.Trim();
        if (trimmed.Length != 3 || !trimmed.All(char.IsAsciiLetter))
        {
            return false;
        }

        normalized = trimmed.ToUpperInvariant();
        return true;
    }
}
