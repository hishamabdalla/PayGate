namespace PayGate;

using System.Diagnostics.CodeAnalysis;

/// <summary>The outcome of a gateway operation: a value or a <see cref="PaymentError"/>. Expected failures never throw.</summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class PaymentResult<T>
{
    private readonly T? _value;

    internal PaymentResult(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    internal PaymentResult(PaymentError error)
    {
        Error = error;
    }

    /// <summary>Whether the operation succeeded.</summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>Whether the operation failed.</summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    /// <summary>The value of a successful operation.</summary>
    /// <exception cref="InvalidOperationException">The operation failed. Check <see cref="IsSuccess"/> first.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"The operation failed with {Error.Code}: {Error.Message}. Check IsSuccess before reading Value.");

    /// <summary>The error of a failed operation; <see langword="null"/> on success.</summary>
    public PaymentError? Error { get; }

    /// <summary>Wraps a value in a successful result.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static implicit operator PaymentResult<T>(T value) => PaymentResult.Success(value);

    /// <summary>Wraps an error in a failed result.</summary>
    /// <param name="error">The error.</param>
    /// <returns>The result.</returns>
    public static implicit operator PaymentResult<T>(PaymentError error) => PaymentResult.Failure<T>(error);

    /// <summary>Maps the result to one value, whichever way it went.</summary>
    /// <typeparam name="TResult">The type to map to.</typeparam>
    /// <param name="onSuccess">Called with the value on success.</param>
    /// <param name="onFailure">Called with the error on failure.</param>
    /// <returns>What the called function returned.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<PaymentError, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value!) : onFailure(Error);
    }

    /// <summary>Returns <c>Success(value)</c> or <c>Failure(code: message)</c>.</summary>
    /// <returns>A short description of the result.</returns>
    public override string ToString() => IsSuccess ? $"Success({_value})" : $"Failure({Error.Code}: {Error.Message})";
}

/// <summary>Creates <see cref="PaymentResult{T}"/> values.</summary>
public static class PaymentResult
{
    /// <summary>Wraps a value in a successful result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value. Must not be <see langword="null"/>.</param>
    /// <returns>The result.</returns>
    public static PaymentResult<T> Success<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new PaymentResult<T>(value);
    }

    /// <summary>Wraps an error in a failed result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>The result.</returns>
    public static PaymentResult<T> Failure<T>(PaymentError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new PaymentResult<T>(error);
    }
}
