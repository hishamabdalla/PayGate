namespace PayGate;

using Microsoft.Extensions.Options;

/// <summary>The default credentials provider: the account's named options, read through <see cref="IOptionsMonitor{TOptions}"/>.</summary>
internal sealed class OptionsCredentialsProvider<TOptions>(IOptionsMonitor<TOptions> options) : IPaymentCredentialsProvider<TOptions>
    where TOptions : PaymentAccountOptions
{
    public ValueTask<TOptions> GetCredentialsAsync(PaymentAccount account, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(options.Get(account.ToString()));
}
