namespace PayGate;

/// <summary>What the app must do next to let the customer pay.</summary>
/// <remarks>Values may be added in minor versions. Handle unknown values defensively.</remarks>
public enum NextActionKind
{
    /// <summary>Nothing: the payment needs no customer action.</summary>
    None = 0,

    /// <summary>Send the customer to a URL (a hosted page, a 3DS page) or embed it in an iframe.</summary>
    Redirect,

    /// <summary>Hand a client secret to a provider SDK on the web or in a mobile app.</summary>
    ClientSdk,

    /// <summary>Show the customer a reference code to pay later, for example at a kiosk.</summary>
    DisplayReference,
}
