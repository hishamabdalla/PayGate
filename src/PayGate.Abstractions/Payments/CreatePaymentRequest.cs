namespace PayGate;

using System.Collections.Immutable;

/// <summary>A request to start a payment.</summary>
/// <remarks>
/// Optional fields a provider cannot honour are ignored, as documented per provider and reported through
/// <see cref="PaymentCapabilities"/>. Fields a provider requires but which are missing produce a
/// <see cref="PaymentErrorCode.Validation"/> error.
/// </remarks>
public sealed record CreatePaymentRequest
{
    /// <summary>
    /// The app's reference for this payment attempt. Must be unique per attempt: a retried checkout needs a new reference.
    /// It is the one correlation key every provider echoes back.
    /// </summary>
    public required string MerchantReference { get; init; }

    /// <summary>The amount to charge.</summary>
    public required Money Amount { get; init; }

    /// <summary>A short description shown to the customer where the provider supports it.</summary>
    public string? Description { get; init; }

    /// <summary>The customer.</summary>
    public Customer? Customer { get; init; }

    /// <summary>Order lines. When a provider requires items and none are given, the gateway sends one built from <see cref="Description"/> and <see cref="Amount"/>.</summary>
    public IReadOnlyList<LineItem> LineItems { get; init; } = [];

    /// <summary>The payment methods to offer. Empty means the account's default set.</summary>
    public IReadOnlySet<PaymentMethodType> AllowedPaymentMethods { get; init; } = ImmutableHashSet<PaymentMethodType>.Empty;

    /// <summary>Where the customer returns after paying. The return is informational only: confirm the status before fulfilling.</summary>
    public Uri? ReturnUrl { get; init; }

    /// <summary>Where the customer goes after cancelling or failing, where the provider supports it.</summary>
    public Uri? CancelUrl { get; init; }

    /// <summary>A webhook URL for this payment only. Honoured only when <see cref="PaymentCapabilities.PerRequestNotificationUrl"/> is set.</summary>
    public Uri? NotificationUrl { get; init; }

    /// <summary>When the payment expires. Honoured only when <see cref="PaymentCapabilities.PaymentExpiry"/> is set.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The language for the provider's pages, such as <c>ar</c> or <c>en</c>, where supported.</summary>
    public string? Locale { get; init; }

    /// <summary>String metadata sent to the provider. Echoed back only when <see cref="PaymentCapabilities.MetadataPassthrough"/> is set.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// The idempotency key for providers that honour one (<see cref="PaymentCapabilities.IdempotentWrites"/>).
    /// Defaults to <see cref="MerchantReference"/>.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>Options for one provider. Must match the gateway's provider.</summary>
    public ProviderPaymentOptions? ProviderOptions { get; init; }
}
