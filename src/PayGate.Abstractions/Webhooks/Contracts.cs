namespace PayGate.Webhooks;

using System.Diagnostics.CodeAnalysis;

/// <summary>Verifies a provider's webhook signature and parses the payload into normalized events. One per provider.</summary>
public interface IWebhookVerifier
{
    /// <summary>The provider this verifier handles.</summary>
    PaymentProviderId Provider { get; }

    /// <summary>Verifies the signature over the raw request, then parses the events. Never throws for a bad request.</summary>
    /// <param name="request">The webhook as received.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The outcome. Events are returned only when the signature is valid.</returns>
    ValueTask<WebhookParseResult> VerifyAndParseAsync(WebhookRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Verifies and reads a provider's return callback. One per provider.</summary>
public interface IPaymentReturnParser
{
    /// <summary>The provider this parser handles.</summary>
    PaymentProviderId Provider { get; }

    /// <summary>Verifies the query signature, where the provider signs one, and reads what the callback claims.</summary>
    /// <param name="request">The return callback's query.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>What the callback claims. Untrusted.</returns>
    ValueTask<ReturnCallbackResult> ParseAsync(ReturnCallbackRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implemented by the app: applies a verified payment event. Called in a fresh DI scope for each event.
/// </summary>
/// <remarks>
/// The app deduplicates on <see cref="PaymentEvent.EventId"/> (returning <see cref="WebhookHandlingOutcome.AlreadyProcessed"/>),
/// checks the status transition, and fulfils. Throwing makes the endpoint answer 500 so that the provider retries.
/// </remarks>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "An interface the app implements to handle payment events, not a .NET event delegate. The name is part of the documented consumer API.")]
public interface IPaymentEventHandler
{
    /// <summary>Applies one event.</summary>
    /// <param name="paymentEvent">The verified event.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>What the app did with the event.</returns>
    ValueTask<WebhookHandlingOutcome> HandleAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default);
}
