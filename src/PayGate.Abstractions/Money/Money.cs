namespace PayGate;

using System.Globalization;

/// <summary>
/// A non-negative amount in a currency, with no more decimal places than the currency's minor unit allows.
/// </summary>
/// <remarks>
/// Conversions to minor units are exact because the scale is validated on construction. Use
/// <see cref="Round"/> to turn a computed amount into money: the library never truncates.
/// </remarks>
public readonly record struct Money : IComparable<Money>
{
    private static readonly string[] _formats = ["F0", "F1", "F2", "F3", "F4"];

    /// <summary>Creates an amount of money.</summary>
    /// <param name="amount">The amount. Must be non-negative, with at most <see cref="Currency.MinorUnitExponent"/> decimal places.</param>
    /// <param name="currency">The currency.</param>
    /// <exception cref="ArgumentException">The currency is <see langword="default"/>, or the amount has too many decimal places.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The amount is negative.</exception>
    public Money(decimal amount, Currency currency)
    {
        if (currency.Code.Length == 0)
        {
            throw new ArgumentException("A currency is required.", nameof(currency));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount != Math.Round(amount, currency.MinorUnitExponent))
        {
            throw new ArgumentException(
                $"{amount.ToString(CultureInfo.InvariantCulture)} has more than {currency.MinorUnitExponent} decimal places, which {currency.Code} does not allow. Use Money.Round to round it.",
                nameof(amount));
        }

        Amount = amount;
        Currency = currency;
    }

    /// <summary>The amount in major units, for example <c>150.25</c>.</summary>
    public decimal Amount { get; }

    /// <summary>The currency.</summary>
    public Currency Currency { get; }

    /// <summary>Whether the amount is zero.</summary>
    public bool IsZero => Amount == 0m;

    /// <summary>Returns zero in a currency.</summary>
    /// <param name="currency">The currency.</param>
    /// <returns>Zero.</returns>
    public static Money Zero(Currency currency) => new(0m, currency);

    /// <summary>Rounds a computed amount to the currency's minor unit. This is the only rounding entry point.</summary>
    /// <param name="amount">The amount, with any number of decimal places.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="mode">The midpoint rounding mode. Defaults to away from zero.</param>
    /// <returns>The rounded money.</returns>
    public static Money Round(decimal amount, Currency currency, MidpointRounding mode = MidpointRounding.AwayFromZero) =>
        new(Math.Round(amount, currency.MinorUnitExponent, mode), currency);

    /// <summary>Creates money from an integer number of minor units, for example cents.</summary>
    /// <param name="minorUnits">The number of minor units. Must be non-negative.</param>
    /// <param name="currency">The currency.</param>
    /// <returns>The money.</returns>
    public static Money FromMinorUnits(long minorUnits, Currency currency) => new(minorUnits / currency.MinorUnitFactor, currency);

    /// <summary>Returns the amount as an integer number of minor units, for example cents. Exact, never rounded.</summary>
    /// <returns>The number of minor units.</returns>
    /// <exception cref="OverflowException">The amount does not fit in a <see cref="long"/>.</exception>
    public long ToMinorUnits() => (long)(Amount * Currency.MinorUnitFactor);

    /// <summary>Adds two amounts in the same currency.</summary>
    /// <param name="other">The amount to add.</param>
    /// <returns>The sum.</returns>
    /// <exception cref="InvalidOperationException">The currencies differ.</exception>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>Subtracts an amount in the same currency.</summary>
    /// <param name="other">The amount to subtract.</param>
    /// <returns>The difference.</returns>
    /// <exception cref="InvalidOperationException">The currencies differ, or the result would be negative.</exception>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return other.Amount > Amount
            ? throw new InvalidOperationException($"Subtracting {other} from {this} would be negative.")
            : new Money(Amount - other.Amount, Currency);
    }

    /// <summary>Compares two amounts in the same currency.</summary>
    /// <param name="other">The amount to compare with.</param>
    /// <returns>Less than zero, zero or greater than zero.</returns>
    /// <exception cref="InvalidOperationException">The currencies differ.</exception>
    public int CompareTo(Money other)
    {
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    /// <summary>Returns the amount and currency in invariant culture, for example <c>150.00 EGP</c>.</summary>
    /// <returns>The formatted amount.</returns>
    public override string ToString() =>
        $"{Amount.ToString(_formats[Currency.MinorUnitExponent], CultureInfo.InvariantCulture)} {Currency.Code}";

    /// <summary>Adds two amounts in the same currency.</summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The sum.</returns>
    public static Money operator +(Money left, Money right) => left.Add(right);

    /// <summary>Subtracts an amount in the same currency.</summary>
    /// <param name="left">The amount to subtract from.</param>
    /// <param name="right">The amount to subtract.</param>
    /// <returns>The difference.</returns>
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    /// <summary>Whether the left amount is less than the right one.</summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The comparison result.</returns>
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    /// <summary>Whether the left amount is greater than the right one.</summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The comparison result.</returns>
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    /// <summary>Whether the left amount is less than or equal to the right one.</summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The comparison result.</returns>
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    /// <summary>Whether the left amount is greater than or equal to the right one.</summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    /// <returns>The comparison result.</returns>
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot combine {Currency.Code} with {other.Currency.Code}.");
        }
    }
}
