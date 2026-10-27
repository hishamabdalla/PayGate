namespace PayGate;

/// <summary>A request to refund all or part of a payment.</summary>
/// <remarks>
/// Each provider validates the ids it needs, and may make one safe read first to resolve a missing id or the
/// remaining amount. Refunds are asynchronous on every provider: <see cref="RefundStatus.Pending"/> is a normal result.
/// </remarks>
public sealed record RefundRequest
{
    /// <summary>The id the provider returned at creation.</summary>
    public required string ProviderPaymentId { get; init; }

    /// <summary>The money-moving transaction to refund, from <see cref="PaymentDetails.ProviderTransactionId"/> or a payment event.</summary>
    public string? ProviderTransactionId { get; init; }

    /// <summary>The amount to refund. <see langword="null"/> refunds whatever remains.</summary>
    public Money? Amount { get; init; }

    /// <summary>A free-text reason, passed on where the provider supports one.</summary>
    public string? Reason { get; init; }

    /// <summary>
    /// The app's reference for this refund. It prevents a duplicate refund only when
    /// <see cref="PaymentCapabilities.IdempotentWrites"/> is set; elsewhere it is used for correlation.
    /// </summary>
    public string? MerchantRefundReference { get; init; }
}
