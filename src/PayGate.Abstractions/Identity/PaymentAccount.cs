namespace PayGate;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A merchant account at one provider: the provider plus an account name. One gateway instance serves one account.
/// </summary>
/// <remarks>
/// The string form is <c>kashier</c> for the default account and <c>kashier:secondary</c> otherwise.
/// The same string is the keyed-service key and the account segment of the webhook route.
/// </remarks>
public readonly record struct PaymentAccount
{
    /// <summary>The name of the account used when none is given.</summary>
    public const string DefaultName = "default";

    private const char _separator = ':';

    /// <summary>Creates an account.</summary>
    /// <param name="provider">The provider.</param>
    /// <param name="name">The account name. Case-insensitive; letters, digits, <c>-</c> and <c>_</c>.</param>
    /// <exception cref="ArgumentException">The provider is <see langword="default"/> or the name is invalid.</exception>
    public PaymentAccount(PaymentProviderId provider, string name = DefaultName)
    {
        if (provider.Value.Length == 0)
        {
            throw new ArgumentException("A provider is required.", nameof(provider));
        }

        Provider = provider;
        Name = IdentifierRules.Normalize(name, allowUnderscore: true, nameof(name));
    }

    /// <summary>The provider.</summary>
    public PaymentProviderId Provider { get; }

    /// <summary>The normalized, lowercase account name.</summary>
    public string Name { get => field ?? DefaultName; private init; }

    /// <summary>Whether this is the provider's default account.</summary>
    public bool IsDefault => Name == DefaultName;

    /// <summary>Parses the string form produced by <see cref="ToString"/>.</summary>
    /// <param name="value"><c>provider</c> or <c>provider:account</c>.</param>
    /// <returns>The account.</returns>
    /// <exception cref="FormatException">The value is not a valid account.</exception>
    public static PaymentAccount Parse(string value) =>
        TryParse(value, out var account)
            ? account
            : throw new FormatException($"'{value}' is not a valid payment account. Expected 'provider' or 'provider:account'.");

    /// <summary>Tries to parse the string form produced by <see cref="ToString"/>.</summary>
    /// <param name="value"><c>provider</c> or <c>provider:account</c>.</param>
    /// <param name="account">The parsed account, when successful.</param>
    /// <returns><see langword="true"/> when the value is a valid account.</returns>
    public static bool TryParse([NotNullWhen(true)] string? value, out PaymentAccount account)
    {
        account = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        int separator = value.IndexOf(_separator, StringComparison.Ordinal);
        string providerPart = separator < 0 ? value : value[..separator];
        string namePart = separator < 0 ? DefaultName : value[(separator + 1)..];

        try
        {
            account = new PaymentAccount(new PaymentProviderId(providerPart), namePart);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>Returns <c>provider</c> for the default account and <c>provider:account</c> otherwise.</summary>
    /// <returns>The string form of the account.</returns>
    public override string ToString() => IsDefault ? Provider.Value : $"{Provider.Value}{_separator}{Name}";
}
