namespace PayGate;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A payment gateway for one provider account. Gateways are stateless; resolve them with <see cref="IPaymentGatewayResolver"/>.
/// </summary>
/// <remarks>
/// Every operation returns a <see cref="PaymentResult{T}"/> and never throws for an expected failure. A write that times out
/// returns <see cref="PaymentErrorCode.OutcomeUnknown"/>.
/// </remarks>
public interface IPaymentGateway
{
    /// <summary>The account this gateway serves.</summary>
    PaymentAccount Account { get; }

    /// <summary>What the account supports.</summary>
    PaymentCapabilities Capabilities { get; }

    /// <summary>Starts a payment.</summary>
    /// <param name="request">The payment to create.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The session, including what the app must do next.</returns>
    Task<PaymentResult<PaymentSession>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reads a payment's current state from the provider.</summary>
    /// <param name="lookup">The payment to read.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The payment's details.</returns>
    Task<PaymentResult<PaymentDetails>> GetPaymentAsync(PaymentLookup lookup, CancellationToken cancellationToken = default);

    /// <summary>Refunds all or part of a payment.</summary>
    /// <param name="request">The refund to make.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The refund's result, often <see cref="RefundStatus.Pending"/>.</returns>
    Task<PaymentResult<RefundResult>> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns an optional feature, such as <see cref="IPaymentCancelFeature"/>, when the gateway supports it.</summary>
    /// <typeparam name="TFeature">The feature interface.</typeparam>
    /// <param name="feature">The feature, when supported.</param>
    /// <returns><see langword="true"/> when the gateway supports the feature.</returns>
    bool TryGetFeature<TFeature>([NotNullWhen(true)] out TFeature? feature)
        where TFeature : class;
}
