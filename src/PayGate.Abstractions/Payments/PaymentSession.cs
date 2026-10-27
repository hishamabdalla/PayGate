namespace PayGate;

using System.Text;

/// <summary>The result of creating a payment.</summary>
/// <remarks><see cref="object.ToString"/> leaves out the raw response.</remarks>
public sealed record PaymentSession
{
    /// <summary>
    /// The id the provider returned at creation. It stays valid for <see cref="IPaymentGateway.GetPaymentAsync"/>, so store it.
    /// </summary>
    public required string ProviderPaymentId { get; init; }

    /// <summary>The normalized status, usually <see cref="PaymentStatus.Pending"/>.</summary>
    public required PaymentStatus Status { get; init; }

    /// <summary>What the app must do next for the customer to pay.</summary>
    public required NextAction NextAction { get; init; }

    /// <summary>When the payment expires, if the provider says.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>The provider's raw response body, for diagnostics. May contain secrets: do not log it.</summary>
    public string? RawResponse { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("ProviderPaymentId = ").Append(ProviderPaymentId)
            .Append(", Status = ").Append(Status)
            .Append(", NextAction = ").Append(NextAction)
            .Append(", ExpiresAt = ").Append(ExpiresAt);
        return true;
    }
}
