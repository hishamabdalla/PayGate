namespace PayGate;

/// <summary>Identifies the payment to look up. Create one with <see cref="ByProviderPaymentId"/> or <see cref="ByMerchantReference"/>.</summary>
public sealed record PaymentLookup
{
    private PaymentLookup(PaymentLookupKind kind, string value)
    {
        Kind = kind;
        Value = value;
    }

    /// <summary>Which id <see cref="Value"/> holds.</summary>
    public PaymentLookupKind Kind { get; }

    /// <summary>The id to look up.</summary>
    public string Value { get; }

    /// <summary>Looks a payment up by the id the provider returned at creation.</summary>
    /// <param name="providerPaymentId">The provider payment id.</param>
    /// <returns>The lookup.</returns>
    public static PaymentLookup ByProviderPaymentId(string providerPaymentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerPaymentId);
        return new PaymentLookup(PaymentLookupKind.ProviderPaymentId, providerPaymentId);
    }

    /// <summary>
    /// Looks a payment up by the app's merchant reference. Supported only when
    /// <see cref="PaymentCapabilities.LookupByMerchantReference"/> is set; otherwise the gateway returns
    /// <see cref="PaymentErrorCode.NotSupported"/>.
    /// </summary>
    /// <param name="merchantReference">The merchant reference sent at creation.</param>
    /// <returns>The lookup.</returns>
    public static PaymentLookup ByMerchantReference(string merchantReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(merchantReference);
        return new PaymentLookup(PaymentLookupKind.MerchantReference, merchantReference);
    }
}

/// <summary>Which id a <see cref="PaymentLookup"/> holds.</summary>
public enum PaymentLookupKind
{
    /// <summary>The id the provider returned at creation.</summary>
    ProviderPaymentId = 0,

    /// <summary>The app's merchant reference.</summary>
    MerchantReference,
}
