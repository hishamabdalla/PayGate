namespace PayGate;

using System.Text;

/// <summary>
/// What the app must do next for the customer to pay. Match on the derived type:
/// <see cref="RedirectAction"/>, <see cref="ClientSdkAction"/>, <see cref="DisplayReferenceAction"/> or <see cref="NoAction"/>.
/// </summary>
/// <remarks>Kinds may be added in minor versions. Handle unknown kinds defensively.</remarks>
public abstract record NextAction
{
    private protected NextAction()
    {
    }

    /// <summary>The kind of action, matching the derived type.</summary>
    public abstract NextActionKind Kind { get; }
}

/// <summary>Send the customer to <see cref="Url"/>, or use it as an iframe source where the provider allows it.</summary>
public sealed record RedirectAction : NextAction
{
    /// <summary>The URL to send the customer to.</summary>
    public required Uri Url { get; init; }

    /// <inheritdoc/>
    public override NextActionKind Kind => NextActionKind.Redirect;
}

/// <summary>Hand <see cref="ClientSecret"/> to the provider's web or mobile SDK.</summary>
/// <remarks><see cref="object.ToString"/> redacts the client secret.</remarks>
public sealed record ClientSdkAction : NextAction
{
    /// <summary>The client secret for the provider's SDK. Send it only to the paying customer's client.</summary>
    public required string ClientSecret { get; init; }

    /// <summary>The provider's public or publishable key, when the SDK needs one.</summary>
    public string? PublicKey { get; init; }

    /// <inheritdoc/>
    public override NextActionKind Kind => NextActionKind.ClientSdk;

    /// <inheritdoc/>
    protected override bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Append("Kind = ").Append(Kind)
            .Append(", ClientSecret = ").Append(Redaction.Masked)
            .Append(", PublicKey = ").Append(PublicKey);
        return true;
    }
}

/// <summary>Show the customer <see cref="Code"/> to pay later, for example at a Fawry kiosk.</summary>
public sealed record DisplayReferenceAction : NextAction
{
    /// <summary>The reference code the customer pays with.</summary>
    public required string Code { get; init; }

    /// <summary>When the code stops being payable, if the provider says.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Instructions to show the customer, if the provider supplies them.</summary>
    public string? Instructions { get; init; }

    /// <inheritdoc/>
    public override NextActionKind Kind => NextActionKind.DisplayReference;
}

/// <summary>No customer action is needed.</summary>
public sealed record NoAction : NextAction
{
    /// <summary>The shared instance.</summary>
    public static NoAction Instance { get; } = new();

    /// <inheritdoc/>
    public override NextActionKind Kind => NextActionKind.None;
}
