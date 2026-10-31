namespace PayGate;

using System.Diagnostics.CodeAnalysis;

/// <summary>Resolves gateways from the registrations made with <see cref="PayGateBuilder.AddGateway"/>.</summary>
internal sealed class PaymentGatewayResolver : IPaymentGatewayResolver
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<PaymentAccount, GatewayRegistration> _registrations;

    public PaymentGatewayResolver(IServiceProvider services, IEnumerable<GatewayRegistration> registrations)
    {
        _services = services;
        _registrations = registrations.ToDictionary(r => r.Account);
    }

    public IPaymentGateway GetGateway(PaymentAccount account)
    {
        if (TryGetGateway(account, out var gateway))
        {
            return gateway;
        }

        string registered = _registrations.Count == 0
            ? "none"
            : string.Join(", ", _registrations.Keys.Select(a => a.ToString()).Order(StringComparer.Ordinal));
        throw new InvalidOperationException(
            $"No payment gateway is registered for '{account}'. Registered accounts: {registered}. " +
            $"Register it on services.AddPayGate() with the provider package's method, for example AddKashier(...).");
    }

    public bool TryGetGateway(PaymentAccount account, [NotNullWhen(true)] out IPaymentGateway? gateway)
    {
        if (_registrations.TryGetValue(account, out var registration))
        {
            gateway = registration.Factory(_services);
            return true;
        }

        gateway = null;
        return false;
    }
}
