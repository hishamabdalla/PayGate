namespace PayGate;

/// <summary>Whether an account talks to the provider's sandbox or to live payments.</summary>
public enum PaymentEnvironment
{
    /// <summary>The provider's test environment. The default, so a missing setting never moves real money.</summary>
    Sandbox = 0,

    /// <summary>Live payments.</summary>
    Live,
}
