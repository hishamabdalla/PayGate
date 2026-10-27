namespace PayGate;

using System.Text;

/// <summary>The customer making a payment. Each provider validates the fields it requires.</summary>
/// <remarks><see cref="object.ToString"/> redacts the name, email and phone.</remarks>
public sealed record Customer
{
    /// <summary>The app's own id for the customer.</summary>
    public string? Reference { get; init; }

    /// <summary>The customer's full name.</summary>
    public string? Name { get; init; }

    /// <summary>The customer's email address.</summary>
    public string? Email { get; init; }

    /// <summary>The customer's phone number, preferably in E.164 format.</summary>
    public string? Phone { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Reference = ").Append(Reference)
            .Append(", Name = ").Append(Redaction.Mask(Name))
            .Append(", Email = ").Append(Redaction.Mask(Email))
            .Append(", Phone = ").Append(Redaction.Mask(Phone));
        return true;
    }
}
