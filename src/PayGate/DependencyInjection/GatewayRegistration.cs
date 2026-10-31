namespace PayGate;

/// <summary>One registered account and how to create its gateway. Kept in the service collection so the resolver can list them.</summary>
internal sealed record GatewayRegistration(PaymentAccount Account, Func<IServiceProvider, IPaymentGateway> Factory);
