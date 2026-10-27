namespace PayGate;

/// <summary>
/// Base type for options that only one provider understands, such as Kashier's <c>maxFailureAttempts</c>
/// or a Paymob integration id. Each provider package derives its own type.
/// </summary>
/// <remarks>A gateway that receives options for a different provider returns a <see cref="PaymentErrorCode.Validation"/> error.</remarks>
public abstract record ProviderPaymentOptions
{
    /// <summary>The provider these options are for.</summary>
    public abstract PaymentProviderId Provider { get; }
}
