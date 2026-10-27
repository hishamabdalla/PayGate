namespace PayGate;

using System.Diagnostics.CodeAnalysis;

/// <summary>Resolves the gateway for a provider account.</summary>
public interface IPaymentGatewayResolver
{
    /// <summary>Returns the gateway for an account.</summary>
    /// <param name="account">The account.</param>
    /// <returns>The gateway.</returns>
    /// <exception cref="InvalidOperationException">No gateway is registered for the account. The message names the missing registration.</exception>
    IPaymentGateway GetGateway(PaymentAccount account);

    /// <summary>Tries to return the gateway for an account.</summary>
    /// <param name="account">The account.</param>
    /// <param name="gateway">The gateway, when registered.</param>
    /// <returns><see langword="true"/> when a gateway is registered for the account.</returns>
    bool TryGetGateway(PaymentAccount account, [NotNullWhen(true)] out IPaymentGateway? gateway);
}
