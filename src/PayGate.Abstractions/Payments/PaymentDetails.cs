namespace PayGate;

/// <summary>The current state of a payment, as reported by the provider.</summary>
public sealed record PaymentDetails
{
    /// <summary>The id the provider returned at creation.</summary>
    public required string ProviderPaymentId { get; init; }

    /// <summary>The id of the money-moving transaction, once the customer has paid. The refund target on most providers.</summary>
    public string? ProviderTransactionId { get; init; }

    /// <summary>The app's merchant reference, when the provider returns it.</summary>
    public string? MerchantReference { get; init; }

    /// <summary>The normalized status.</summary>
    public required PaymentStatus Status { get; init; }

    /// <summary>The provider's own status, exactly as received. Always kept, even when <see cref="Status"/> is <see cref="PaymentStatus.Unknown"/>.</summary>
    public required string RawStatus { get; init; }

    /// <summary>The amount of the payment.</summary>
    public required Money Amount { get; init; }

    /// <summary>The amount captured, when the provider reports it separately.</summary>
    public Money? CapturedAmount { get; init; }

    /// <summary>The total refunded so far. Zero when nothing has been refunded.</summary>
    public required Money RefundedAmount { get; init; }

    /// <summary>How the customer paid, when known.</summary>
    public PaymentMethodInfo? PaymentMethod { get; init; }

    /// <summary>Why the payment failed, when it did.</summary>
    public PaymentFailureReason? Failure { get; init; }

    /// <summary>When the payment was created, if the provider says.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the payment last changed, if the provider says.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Provider-specific details, such as Kashier settlement information. Read it with <see cref="GetProviderData{T}"/>.</summary>
    public object? ProviderData { get; init; }

    /// <summary>Returns <see cref="ProviderData"/> as the provider package's type.</summary>
    /// <typeparam name="T">The provider package's details type.</typeparam>
    /// <returns>The details, or <see langword="null"/> when absent or of another type.</returns>
    public T? GetProviderData<T>()
        where T : class => ProviderData as T;
}

/// <summary>How a customer paid. Never contains a full card number.</summary>
public sealed record PaymentMethodInfo
{
    /// <summary>The kind of method.</summary>
    public required PaymentMethodType Type { get; init; }

    /// <summary>The card brand or wallet name, such as <c>visa</c>.</summary>
    public string? Brand { get; init; }

    /// <summary>The last four digits of the card, when the method is a card.</summary>
    public string? Last4 { get; init; }
}

/// <summary>Why a payment failed.</summary>
public sealed record PaymentFailureReason
{
    /// <summary>The normalized reason, usually <see cref="PaymentErrorCode.Declined"/>.</summary>
    public required PaymentErrorCode Code { get; init; }

    /// <summary>The provider's own code.</summary>
    public string? ProviderCode { get; init; }

    /// <summary>The provider's message, in English.</summary>
    public string? Message { get; init; }

    /// <summary>The provider's message in Arabic, when it supplies one.</summary>
    public string? MessageAr { get; init; }
}
