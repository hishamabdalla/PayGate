namespace PayGate.Webhooks;

/// <summary>What a payment event reports.</summary>
/// <remarks>Values may be added in minor versions. Handle unknown values defensively.</remarks>
public enum PaymentEventType
{
    /// <summary>An event the library does not classify. The webhook is acknowledged as ignored.</summary>
    Other = 0,

    /// <summary>The payment is waiting for the customer or for confirmation.</summary>
    PaymentPending,

    /// <summary>Funds are held and must be captured.</summary>
    PaymentAuthorized,

    /// <summary>The payment succeeded and money moved.</summary>
    PaymentSucceeded,

    /// <summary>The payment failed.</summary>
    PaymentFailed,

    /// <summary>The payment was cancelled or voided.</summary>
    PaymentCanceled,

    /// <summary>The payment expired before the customer paid.</summary>
    PaymentExpired,

    /// <summary>A refund succeeded. See <see cref="PaymentEvent.RefundAmount"/> and <see cref="PaymentEvent.RefundedTotal"/>.</summary>
    RefundSucceeded,

    /// <summary>A refund failed.</summary>
    RefundFailed,
}
