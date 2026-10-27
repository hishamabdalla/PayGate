namespace PayGate;

/// <summary>The normalized reason an operation failed.</summary>
/// <remarks>Values may be added in minor versions. Handle unknown values defensively.</remarks>
public enum PaymentErrorCode
{
    /// <summary>The provider returned an error that maps to no other code.</summary>
    ProviderError = 0,

    /// <summary>The request is invalid, for example a field the provider requires is missing.</summary>
    Validation,

    /// <summary>The account is misconfigured, for example a live key against the sandbox host.</summary>
    Configuration,

    /// <summary>The provider rejected the credentials.</summary>
    Authentication,

    /// <summary>The payment or refund was declined.</summary>
    Declined,

    /// <summary>The payment was not found.</summary>
    NotFound,

    /// <summary>The merchant reference was already used for a different request.</summary>
    DuplicateReference,

    /// <summary>The provider, account, payment method or payment state does not support the operation.</summary>
    NotSupported,

    /// <summary>The provider is rate-limiting requests.</summary>
    RateLimited,

    /// <summary>The provider is unavailable.</summary>
    ProviderUnavailable,

    /// <summary>
    /// A write timed out or failed in transport after it may have reached the provider. The payment or refund may exist.
    /// Reconcile by lookup or by an idempotent retry; never retry blindly.
    /// </summary>
    OutcomeUnknown,
}
