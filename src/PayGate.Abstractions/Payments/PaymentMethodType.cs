namespace PayGate;

/// <summary>A kind of payment method.</summary>
/// <remarks>Values may be added in minor versions. Handle unknown values defensively.</remarks>
public enum PaymentMethodType
{
    /// <summary>A method not covered by the other values.</summary>
    Other = 0,

    /// <summary>Debit or credit card, including Apple Pay and Google Pay.</summary>
    Card,

    /// <summary>Mobile wallet, such as Vodafone Cash.</summary>
    Wallet,

    /// <summary>InstaPay instant transfer.</summary>
    InstaPay,

    /// <summary>A reference code paid later in cash, at a kiosk or agent (PayAtFawry, Aman, Basata).</summary>
    CashReference,

    /// <summary>Buy now, pay later (valU, Klarna).</summary>
    Bnpl,
}
