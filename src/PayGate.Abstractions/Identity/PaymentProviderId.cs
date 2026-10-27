namespace PayGate;

/// <summary>
/// Identifies a payment provider, such as <c>kashier</c> or <c>stripe</c>.
/// </summary>
/// <remarks>
/// String-backed rather than an enum, so adding a provider package is never a breaking change.
/// Values are normalized to lowercase and may contain only <c>a-z</c>, <c>0-9</c> and <c>-</c>.
/// </remarks>
public readonly record struct PaymentProviderId
{
    /// <summary>Creates a provider id from its string form.</summary>
    /// <param name="value">The provider id, for example <c>kashier</c>. Case-insensitive.</param>
    /// <exception cref="ArgumentException">The value is empty or contains characters other than letters, digits and <c>-</c>.</exception>
    public PaymentProviderId(string value)
    {
        Value = IdentifierRules.Normalize(value, allowUnderscore: false, nameof(value));
    }

    /// <summary>The normalized, lowercase provider id. Empty for <see langword="default"/>.</summary>
    public string Value { get => field ?? string.Empty; private init; }

    /// <summary>Kashier.</summary>
    public static PaymentProviderId Kashier { get; } = new("kashier");

    /// <summary>Paymob.</summary>
    public static PaymentProviderId Paymob { get; } = new("paymob");

    /// <summary>Fawry.</summary>
    public static PaymentProviderId Fawry { get; } = new("fawry");

    /// <summary>Stripe.</summary>
    public static PaymentProviderId Stripe { get; } = new("stripe");

    /// <summary>Returns the normalized provider id.</summary>
    /// <returns>The provider id, for example <c>kashier</c>.</returns>
    public override string ToString() => Value;
}
