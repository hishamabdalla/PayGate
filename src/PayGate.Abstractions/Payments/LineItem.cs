namespace PayGate;

/// <summary>One line of the order being paid for.</summary>
public sealed record LineItem
{
    /// <summary>The item's display name.</summary>
    public required string Name { get; init; }

    /// <summary>The price of one unit.</summary>
    public required Money UnitPrice { get; init; }

    /// <summary>The number of units. Defaults to 1.</summary>
    public int Quantity { get; init; } = 1;
}
