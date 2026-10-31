namespace PayGate;

/// <summary>
/// Supplies an account's settings and credentials to its gateway each time they are needed. The default reads the named
/// options of the account, so configuration reloads and environment-variable overrides apply.
/// </summary>
/// <typeparam name="TOptions">The provider's account options type.</typeparam>
/// <remarks>
/// Register your own implementation to load per-tenant credentials from a database or a key vault. The app owns the
/// storage and any caching; the library never persists secrets.
/// </remarks>
public interface IPaymentCredentialsProvider<TOptions>
    where TOptions : PaymentAccountOptions
{
    /// <summary>Returns the settings and credentials for an account.</summary>
    /// <param name="account">The account.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The account's options.</returns>
    ValueTask<TOptions> GetCredentialsAsync(PaymentAccount account, CancellationToken cancellationToken = default);
}
