namespace PayGate;

/// <summary>Why an operation failed.</summary>
public sealed record PaymentError
{
    /// <summary>The normalized reason.</summary>
    public required PaymentErrorCode Code { get; init; }

    /// <summary>A message for logs and developers. Never contains secrets.</summary>
    public required string Message { get; init; }

    /// <summary>The provider's own error code, such as Kashier's <c>ERR_ORD_02</c>.</summary>
    public string? ProviderCode { get; init; }

    /// <summary>The provider's own error message.</summary>
    public string? ProviderMessage { get; init; }

    /// <summary>Whether retrying the same request later may succeed. Never true for <see cref="PaymentErrorCode.OutcomeUnknown"/>.</summary>
    public bool IsRetryable { get; init; }

    /// <summary>Creates a <see cref="PaymentErrorCode.Validation"/> error.</summary>
    /// <param name="message">What is invalid.</param>
    /// <returns>The error.</returns>
    public static PaymentError Validation(string message) => new() { Code = PaymentErrorCode.Validation, Message = message };

    /// <summary>Creates a <see cref="PaymentErrorCode.NotSupported"/> error.</summary>
    /// <param name="message">What is not supported.</param>
    /// <returns>The error.</returns>
    public static PaymentError NotSupported(string message) => new() { Code = PaymentErrorCode.NotSupported, Message = message };

    /// <summary>Creates a <see cref="PaymentErrorCode.DuplicateReference"/> error.</summary>
    /// <param name="message">Which reference was reused.</param>
    /// <param name="providerCode">The provider's error code, if any.</param>
    /// <returns>The error.</returns>
    public static PaymentError DuplicateReference(string message, string? providerCode = null) =>
        new() { Code = PaymentErrorCode.DuplicateReference, Message = message, ProviderCode = providerCode };

    /// <summary>Creates an <see cref="PaymentErrorCode.OutcomeUnknown"/> error. It is never retryable.</summary>
    /// <param name="message">Which operation timed out or failed in transport.</param>
    /// <returns>The error.</returns>
    public static PaymentError OutcomeUnknown(string message) => new() { Code = PaymentErrorCode.OutcomeUnknown, Message = message };
}
