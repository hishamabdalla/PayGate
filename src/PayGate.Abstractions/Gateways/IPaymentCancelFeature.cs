namespace PayGate;

/// <summary>
/// Stops a payment that has not settled: releases an authorization, voids a same-day capture, or cancels an unpaid order.
/// Get it with <see cref="IPaymentGateway.TryGetFeature"/>.
/// </summary>
public interface IPaymentCancelFeature
{
    /// <summary>Cancels a payment.</summary>
    /// <param name="request">The payment to cancel.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The payment's status after the cancel.</returns>
    Task<PaymentResult<PaymentCancelResult>> CancelAsync(PaymentCancelRequest request, CancellationToken cancellationToken = default);
}
