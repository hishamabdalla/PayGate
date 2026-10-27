namespace PayGate.Webhooks;

/// <summary>What the app's <see cref="IPaymentEventHandler"/> did with an event. The provider's ack policy maps it to an HTTP response.</summary>
public enum WebhookHandlingOutcome
{
    /// <summary>The event was applied.</summary>
    Processed = 0,

    /// <summary>The event's <see cref="PaymentEvent.EventId"/> was already in the app's inbox. Kashier is answered with 409.</summary>
    AlreadyProcessed,

    /// <summary>The app does not act on this event.</summary>
    Ignored,
}
