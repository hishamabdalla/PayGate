namespace PayGate;

/// <summary>A request to stop a payment that has not settled: release an authorization, void a same-day capture, or cancel an unpaid order.</summary>
public sealed record PaymentCancelRequest
{
    /// <summary>The id the provider returned at creation.</summary>
    public required string ProviderPaymentId { get; init; }

    /// <summary>The transaction to void, when the provider needs it.</summary>
    public string? ProviderTransactionId { get; init; }

    /// <summary>A free-text reason, passed on where the provider supports one.</summary>
    public string? Reason { get; init; }
}

/// <summary>The provider's answer to a cancel request.</summary>
public sealed record PaymentCancelResult
{
    /// <summary>
    /// The payment's status after the cancel, normally <see cref="PaymentStatus.Canceled"/>. A cancel after
    /// <see cref="PaymentStatus.Succeeded"/> means the money was returned.
    /// </summary>
    public required PaymentStatus Status { get; init; }

    /// <summary>The provider's own status, exactly as received.</summary>
    public string? RawStatus { get; init; }
}
