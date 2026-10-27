namespace PayGate.Webhooks;

using System.Collections.Immutable;
using System.Text;

/// <summary>
/// A webhook exactly as received: raw body bytes, headers, query string and method. Signatures are verified over these
/// raw values, never over re-serialized JSON.
/// </summary>
/// <remarks><see cref="object.ToString"/> shows the body's length only.</remarks>
public sealed record WebhookRequest
{
    /// <summary>The account the webhook is for, selected by the route.</summary>
    public required PaymentAccount Account { get; init; }

    /// <summary>The HTTP method, such as <c>POST</c>.</summary>
    public required string Method { get; init; }

    /// <summary>The request headers. Use <see cref="GetHeader"/> for a case-insensitive read.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; init; } = ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

    /// <summary>The query-string parameters. Paymob signs through the query. Use <see cref="GetQuery"/> for a case-insensitive read.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Query { get; init; } = ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

    /// <summary>The raw body bytes.</summary>
    public ReadOnlyMemory<byte> Body { get; init; }

    /// <summary>When the webhook was received.</summary>
    public required DateTimeOffset ReceivedAt { get; init; }

    /// <summary>Returns the first value of a header, matching the name case-insensitively.</summary>
    /// <param name="name">The header name.</param>
    /// <returns>The value, or <see langword="null"/> when absent.</returns>
    public string? GetHeader(string name) => ValueLookup.First(Headers, name);

    /// <summary>Returns the first value of a query parameter, matching the name case-insensitively.</summary>
    /// <param name="name">The parameter name.</param>
    /// <returns>The value, or <see langword="null"/> when absent.</returns>
    public string? GetQuery(string name) => ValueLookup.First(Query, name);

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Account = ").Append(Account)
            .Append(", Method = ").Append(Method)
            .Append(", ReceivedAt = ").Append(ReceivedAt)
            .Append(", Body = ").Append(Body.Length).Append(" bytes");
        return true;
    }
}
