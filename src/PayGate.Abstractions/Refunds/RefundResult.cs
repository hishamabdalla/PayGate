namespace PayGate;

/// <summary>The provider's answer to a refund request.</summary>
public sealed record RefundResult
{
    /// <summary>The provider's id for the refund, when it returns one.</summary>
    public string? ProviderRefundId { get; init; }

    /// <summary>The refund's status. <see cref="RefundStatus.Pending"/> is normal: the outcome follows by webhook or lookup.</summary>
    public required RefundStatus Status { get; init; }

    /// <summary>The amount refunded, when the provider reports it.</summary>
    public Money? Amount { get; init; }

    /// <summary>The provider's own status, exactly as received.</summary>
    public string? RawStatus { get; init; }
}

/// <summary>The status of a refund.</summary>
/// <remarks>Values may be added in minor versions. Handle unknown values defensively.</remarks>
public enum RefundStatus
{
    /// <summary>Accepted but not confirmed yet.</summary>
    Pending = 0,

    /// <summary>The money was returned.</summary>
    Succeeded,

    /// <summary>The refund failed.</summary>
    Failed,
}
