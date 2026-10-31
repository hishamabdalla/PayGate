namespace PayGate;

using System.Text;

/// <summary>
/// Base type for one provider account's settings and credentials, such as <c>KashierAccountOptions</c>. Registered as named
/// options, one name per account (<see cref="PaymentAccount.ToString"/>), and validated when the app starts.
/// </summary>
/// <remarks>
/// <see cref="ToString"/> is sealed so that secrets never reach a log by accident: it prints the type, the environment and
/// whatever a derived type appends in <see cref="AppendMembers"/>, which must pass every secret through <see cref="Redact"/>.
/// </remarks>
public abstract class PaymentAccountOptions
{
    /// <summary>Sandbox or live. Defaults to <see cref="PaymentEnvironment.Sandbox"/>.</summary>
    public PaymentEnvironment Environment { get; set; } = PaymentEnvironment.Sandbox;

    /// <summary>Replaces the provider's base URL, for contract tests against a mock server or for a regional host.</summary>
    public Uri? BaseUrlOverride { get; set; }

    /// <summary>Returns the settings with every secret redacted.</summary>
    /// <returns>For example <c>KashierAccountOptions { Environment = Sandbox, MerchantId = MID-1, SecretKey = *** }</c>.</returns>
    public sealed override string ToString()
    {
        var builder = new StringBuilder(GetType().Name)
            .Append(" { Environment = ").Append(Environment);
        if (BaseUrlOverride is not null)
        {
            builder.Append(", BaseUrlOverride = ").Append(BaseUrlOverride);
        }

        AppendMembers(builder);
        return builder.Append(" }").ToString();
    }

    /// <summary>Appends a derived type's members to <see cref="ToString"/>, each as <c>", Name = value"</c>. Pass secrets through <see cref="Redact"/>.</summary>
    /// <param name="builder">The builder to append to.</param>
    protected virtual void AppendMembers(StringBuilder builder)
    {
    }

    /// <summary>Masks a secret for <see cref="ToString"/>.</summary>
    /// <param name="secret">The secret.</param>
    /// <returns><c>***</c> when set, <c>(not set)</c> when empty, so a missing secret is still visible in diagnostics.</returns>
    protected static string Redact(string? secret) => string.IsNullOrEmpty(secret) ? "(not set)" : "***";
}
