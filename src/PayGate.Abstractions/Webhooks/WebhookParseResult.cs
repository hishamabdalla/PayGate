namespace PayGate.Webhooks;

/// <summary>The outcome of verifying and parsing a webhook.</summary>
public sealed class WebhookParseResult
{
    private WebhookParseResult(WebhookParseKind kind, IReadOnlyList<PaymentEvent> events, string? reason)
    {
        Kind = kind;
        Events = events;
        Reason = reason;
    }

    /// <summary>The outcome.</summary>
    public WebhookParseKind Kind { get; }

    /// <summary>The events, when <see cref="Kind"/> is <see cref="WebhookParseKind.Verified"/>; otherwise empty.</summary>
    public IReadOnlyList<PaymentEvent> Events { get; }

    /// <summary>Why the webhook was rejected or ignored. Never contains secrets or the signature.</summary>
    public string? Reason { get; }

    /// <summary>The signature is valid and the payload parsed into events.</summary>
    /// <param name="events">The events.</param>
    /// <returns>The result.</returns>
    public static WebhookParseResult Verified(IReadOnlyList<PaymentEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        return new WebhookParseResult(WebhookParseKind.Verified, events, null);
    }

    /// <summary>The signature is missing or does not match. The handler is never called.</summary>
    /// <param name="reason">Why, without secrets.</param>
    /// <returns>The result.</returns>
    public static WebhookParseResult InvalidSignature(string reason) => new(WebhookParseKind.InvalidSignature, [], reason);

    /// <summary>The payload could not be read.</summary>
    /// <param name="reason">Why, without secrets.</param>
    /// <returns>The result.</returns>
    public static WebhookParseResult Malformed(string reason) => new(WebhookParseKind.Malformed, [], reason);

    /// <summary>The signature is valid, but the event is of a kind the library does not handle.</summary>
    /// <param name="reason">Which event was ignored.</param>
    /// <returns>The result.</returns>
    public static WebhookParseResult Ignored(string reason) => new(WebhookParseKind.Ignored, [], reason);
}

/// <summary>The outcome of verifying and parsing a webhook.</summary>
public enum WebhookParseKind
{
    /// <summary>The payload could not be read.</summary>
    Malformed = 0,

    /// <summary>The signature is valid and the payload parsed into events.</summary>
    Verified,

    /// <summary>The signature is missing or does not match.</summary>
    InvalidSignature,

    /// <summary>The signature is valid, but the event is not handled.</summary>
    Ignored,
}
