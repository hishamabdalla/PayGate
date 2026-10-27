namespace PayGate.Webhooks;

using System.Collections.Immutable;
using System.Text;

/// <summary>A verified, normalized payment event from a webhook.</summary>
/// <remarks>
/// The app deduplicates on <see cref="EventId"/> in its own inbox table, and applies <see cref="Status"/> only when
/// <see cref="PaymentStatusExtensions.CanTransitionTo"/> allows it. Some providers send several events with different
/// ids for one change, so dedupe alone is not enough. <see cref="object.ToString"/> leaves out the metadata and raw payload.
/// </remarks>
public sealed record PaymentEvent
{
    /// <summary>A stable id for deduplication: the provider's event id, or a deterministic hash when the provider has none.</summary>
    public required string EventId { get; init; }

    /// <summary>The account the event belongs to.</summary>
    public required PaymentAccount Account { get; init; }

    /// <summary>What the event reports.</summary>
    public required PaymentEventType Type { get; init; }

    /// <summary>The app's merchant reference, when the provider echoes it.</summary>
    public string? MerchantReference { get; init; }

    /// <summary>The id the provider returned at creation, when the event carries it.</summary>
    public string? ProviderPaymentId { get; init; }

    /// <summary>The money-moving transaction the event is about.</summary>
    public string? ProviderTransactionId { get; init; }

    /// <summary>The payment's normalized status after the event.</summary>
    public required PaymentStatus Status { get; init; }

    /// <summary>The provider's own status or event name, exactly as received.</summary>
    public string? RawStatus { get; init; }

    /// <summary>The payment amount, when the event carries it. Compare it with the amount the app expects.</summary>
    public Money? Amount { get; init; }

    /// <summary>For a refund event, the amount of this refund. At least one of this and <see cref="RefundedTotal"/> is set.</summary>
    public Money? RefundAmount { get; init; }

    /// <summary>For a refund event, the total refunded so far. At least one of this and <see cref="RefundAmount"/> is set.</summary>
    public Money? RefundedTotal { get; init; }

    /// <summary>When the event happened at the provider, or when it was received if the payload has no time.</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>Whether <see cref="OccurredAt"/> is the receipt time because the payload had none.</summary>
    public bool OccurredAtIsReceiptTime { get; init; }

    /// <summary>The metadata echoed by the provider, when it echoes metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>The raw payload, for audit. May contain personal data: do not log it.</summary>
    public string? RawPayload { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("EventId = ").Append(EventId)
            .Append(", Account = ").Append(Account)
            .Append(", Type = ").Append(Type)
            .Append(", MerchantReference = ").Append(MerchantReference)
            .Append(", ProviderPaymentId = ").Append(ProviderPaymentId)
            .Append(", ProviderTransactionId = ").Append(ProviderTransactionId)
            .Append(", Status = ").Append(Status)
            .Append(", RawStatus = ").Append(RawStatus)
            .Append(", Amount = ").Append(Amount)
            .Append(", RefundAmount = ").Append(RefundAmount)
            .Append(", RefundedTotal = ").Append(RefundedTotal)
            .Append(", OccurredAt = ").Append(OccurredAt);
        return true;
    }
}
