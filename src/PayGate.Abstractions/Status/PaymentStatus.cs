namespace PayGate;

/// <summary>The normalized status of a payment. The provider's own status is always kept alongside it.</summary>
/// <remarks>
/// Values may be added in minor versions. Handle unknown values defensively.
/// Use <see cref="PaymentStatusExtensions.CanTransitionTo"/> before applying a status from a webhook or lookup.
/// </remarks>
public enum PaymentStatus
{
    /// <summary>The provider reported a status the library does not recognise. Never overwrites a known status.</summary>
    Unknown = 0,

    /// <summary>Created, waiting for the customer.</summary>
    Pending,

    /// <summary>The customer must complete an extra step, such as 3DS.</summary>
    RequiresAction,

    /// <summary>The customer acted; the provider has not confirmed the outcome yet.</summary>
    Processing,

    /// <summary>Funds are held and must be captured.</summary>
    Authorized,

    /// <summary>The payment succeeded and money moved.</summary>
    Succeeded,

    /// <summary>The payment failed.</summary>
    Failed,

    /// <summary>The payment was cancelled or voided.</summary>
    Canceled,

    /// <summary>The payment expired before the customer paid.</summary>
    Expired,

    /// <summary>Part of the payment was refunded.</summary>
    PartiallyRefunded,

    /// <summary>The whole payment was refunded.</summary>
    Refunded,
}
