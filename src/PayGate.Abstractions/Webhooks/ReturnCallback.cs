namespace PayGate.Webhooks;

using System.Collections.Immutable;

/// <summary>The query string of the customer's browser returning from the provider.</summary>
public sealed record ReturnCallbackRequest
{
    /// <summary>The account the return is for.</summary>
    public required PaymentAccount Account { get; init; }

    /// <summary>The query-string parameters. Use <see cref="GetQuery"/> for a case-insensitive read.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Query { get; init; } = ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

    /// <summary>Returns the first value of a query parameter, matching the name case-insensitively.</summary>
    /// <param name="name">The parameter name.</param>
    /// <returns>The value, or <see langword="null"/> when absent.</returns>
    public string? GetQuery(string name) => ValueLookup.First(Query, name);
}

/// <summary>
/// What a return callback claims. It is informational only, even when <see cref="Verification"/> is
/// <see cref="ReturnCallbackVerification.Valid"/>: call <see cref="IPaymentGateway.GetPaymentAsync"/> before showing a final result,
/// and fulfil only from verified webhooks or lookups.
/// </summary>
public sealed record ReturnCallbackResult
{
    /// <summary>The account the return is for.</summary>
    public required PaymentAccount Account { get; init; }

    /// <summary>Whether the query carried a valid signature.</summary>
    public required ReturnCallbackVerification Verification { get; init; }

    /// <summary>The merchant reference. At least one of this and <see cref="ProviderPaymentId"/> is set.</summary>
    public string? MerchantReference { get; init; }

    /// <summary>The provider payment id, such as a Stripe Checkout Session id. At least one of this and <see cref="MerchantReference"/> is set.</summary>
    public string? ProviderPaymentId { get; init; }

    /// <summary>The status the query claims. Untrusted.</summary>
    public PaymentStatus? IndicatedStatus { get; init; }
}

/// <summary>Whether a return callback was signed, and whether the signature matched.</summary>
public enum ReturnCallbackVerification
{
    /// <summary>The signature does not match. Treat the callback as tampered.</summary>
    Invalid = 0,

    /// <summary>The signature matches.</summary>
    Valid,

    /// <summary>The provider does not sign return callbacks (Stripe), or signs them unreliably (Fawry).</summary>
    NotSigned,
}
