namespace PayGate;

/// <summary>Rules for moving between <see cref="PaymentStatus"/> values.</summary>
/// <remarks>
/// Webhooks arrive duplicated and out of order. Apply a new status to a stored payment only when
/// <see cref="CanTransitionTo"/> allows it.
/// </remarks>
public static class PaymentStatusExtensions
{
    /// <summary>
    /// Whether the status is terminal: <see cref="PaymentStatus.Failed"/>, <see cref="PaymentStatus.Canceled"/>,
    /// <see cref="PaymentStatus.Expired"/> or <see cref="PaymentStatus.Refunded"/>.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns><see langword="true"/> when no further customer action is expected.</returns>
    /// <remarks>A terminal <see cref="PaymentStatus.Failed"/> can still move to a late success. See <see cref="CanTransitionTo"/>.</remarks>
    public static bool IsTerminal(this PaymentStatus status) =>
        status is PaymentStatus.Failed or PaymentStatus.Canceled or PaymentStatus.Expired or PaymentStatus.Refunded;

    /// <summary>Whether a payment in status <paramref name="from"/> may move to <paramref name="to"/>.</summary>
    /// <param name="from">The stored status.</param>
    /// <param name="to">The newly reported status.</param>
    /// <returns><see langword="true"/> when the move is forward and should be applied.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Nothing moves to <see cref="PaymentStatus.Unknown"/>, and staying in the same status is not a transition.</item>
    /// <item><see cref="PaymentStatus.Canceled"/>, <see cref="PaymentStatus.Expired"/> and <see cref="PaymentStatus.Refunded"/> are sticky.</item>
    /// <item><see cref="PaymentStatus.Failed"/> accepts a late <see cref="PaymentStatus.Succeeded"/> or <see cref="PaymentStatus.Authorized"/>, because the customer may retry within one session.</item>
    /// <item><see cref="PaymentStatus.Succeeded"/> may move to a refund status or to <see cref="PaymentStatus.Canceled"/> (a void).</item>
    /// </list>
    /// </remarks>
    public static bool CanTransitionTo(this PaymentStatus from, PaymentStatus to)
    {
        if (to == from || to == PaymentStatus.Unknown)
        {
            return false;
        }

        return from switch
        {
            PaymentStatus.Unknown or PaymentStatus.Pending or PaymentStatus.RequiresAction or PaymentStatus.Processing => true,
            PaymentStatus.Authorized => to is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Canceled or PaymentStatus.Expired,
            PaymentStatus.Succeeded => to is PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded or PaymentStatus.Canceled,
            PaymentStatus.PartiallyRefunded => to is PaymentStatus.Refunded,
            PaymentStatus.Failed => to is PaymentStatus.Succeeded or PaymentStatus.Authorized,
            _ => false,
        };
    }
}
