// Grows with the public API: each phase adds calls here so the packages are exercised under
// strict consumer settings, not just referenced.
using PayGate;
using PayGate.Providers;
using PayGate.Webhooks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPayGate().AddEventHandler<ConsumerEventHandler>();

var app = builder.Build();

app.MapGet("/", (IPaymentGatewayResolver resolver) =>
{
    var amount = new Money(150m, Currency.Egp);
    bool hasKashier = resolver.TryGetGateway(new PaymentAccount(PaymentProviderId.Kashier), out _);
    return $"PayGate consumer check: {WireFormat.ToDecimalString(amount)}, kashier registered: {hasKashier}";
});

await app.RunAsync();

internal sealed class ConsumerEventHandler : IPaymentEventHandler
{
    public ValueTask<WebhookHandlingOutcome> HandleAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(WebhookHandlingOutcome.Processed);
}
