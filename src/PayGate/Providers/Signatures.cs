namespace PayGate.Providers;

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Hashing and signature helpers for provider packages. Every signature comparison must go through
/// <see cref="FixedTimeEqualsHex"/> or <see cref="FixedTimeEqualsBase64"/>, never a string comparison.
/// </summary>
public static class Signatures
{
    private const int _stackLimit = 128;

    /// <summary>Computes HMAC-SHA256 (Kashier, Stripe).</summary>
    /// <param name="key">The secret key.</param>
    /// <param name="data">The signed data.</param>
    /// <returns>The 32-byte MAC.</returns>
    public static byte[] HmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data) => HMACSHA256.HashData(key, data);

    /// <summary>Computes HMAC-SHA256 over the UTF-8 bytes of a key and data string.</summary>
    /// <param name="key">The secret key.</param>
    /// <param name="data">The signed data.</param>
    /// <returns>The 32-byte MAC.</returns>
    public static byte[] HmacSha256(string key, string data)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(data);
        return HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data));
    }

    /// <summary>Computes HMAC-SHA512 (Paymob).</summary>
    /// <param name="key">The secret key.</param>
    /// <param name="data">The signed data.</param>
    /// <returns>The 64-byte MAC.</returns>
    public static byte[] HmacSha512(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data) => HMACSHA512.HashData(key, data);

    /// <summary>Computes HMAC-SHA512 over the UTF-8 bytes of a key and data string.</summary>
    /// <param name="key">The secret key.</param>
    /// <param name="data">The signed data.</param>
    /// <returns>The 64-byte MAC.</returns>
    public static byte[] HmacSha512(string key, string data)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(data);
        return HMACSHA512.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data));
    }

    /// <summary>Computes a plain SHA-256 hash (Fawry signs <c>SHA-256(fields + secureKey)</c>, not an HMAC).</summary>
    /// <param name="data">The data.</param>
    /// <returns>The 32-byte hash.</returns>
    public static byte[] Sha256(ReadOnlySpan<byte> data) => SHA256.HashData(data);

    /// <summary>Computes a plain SHA-256 hash over the UTF-8 bytes of a string.</summary>
    /// <param name="data">The data.</param>
    /// <returns>The 32-byte hash.</returns>
    public static byte[] Sha256(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return SHA256.HashData(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>Returns the lowercase hex SHA-256 of a raw webhook body, for storing as an audit record instead of the body itself.</summary>
    /// <param name="body">The raw body bytes.</param>
    /// <returns>64 lowercase hex characters.</returns>
    public static string BodyHash(ReadOnlySpan<byte> body) => Convert.ToHexStringLower(SHA256.HashData(body));

    /// <summary>
    /// Compares a computed MAC with a hex signature received from a provider, in time that does not depend on where they differ.
    /// Upper- and lowercase hex are accepted.
    /// </summary>
    /// <param name="expected">The MAC computed locally.</param>
    /// <param name="actualHex">The hex signature from the request.</param>
    /// <returns><see langword="true"/> only when the signature is well-formed hex of the same length and equal bytes. Never throws for bad input.</returns>
    public static bool FixedTimeEqualsHex(ReadOnlySpan<byte> expected, string? actualHex)
    {
        if (actualHex is null || expected.IsEmpty || actualHex.Length != expected.Length * 2)
        {
            return false;
        }

        byte[]? rented = null;
        var actual = expected.Length <= _stackLimit
            ? stackalloc byte[expected.Length]
            : (rented = ArrayPool<byte>.Shared.Rent(expected.Length)).AsSpan(0, expected.Length);
        try
        {
            var status = Convert.FromHexString(actualHex, actual, out _, out int written);
            return status == OperationStatus.Done
                && written == expected.Length
                && CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<byte>.Shared.Return(rented, clearArray: true);
            }
        }
    }

    /// <summary>
    /// Compares a computed MAC with a base64 signature received from a provider, in time that does not depend on where they differ.
    /// </summary>
    /// <param name="expected">The MAC computed locally.</param>
    /// <param name="actualBase64">The base64 signature from the request.</param>
    /// <returns><see langword="true"/> only when the signature is well-formed base64 of equal bytes. Never throws for bad input.</returns>
    public static bool FixedTimeEqualsBase64(ReadOnlySpan<byte> expected, string? actualBase64)
    {
        if (string.IsNullOrEmpty(actualBase64) || expected.IsEmpty)
        {
            return false;
        }

        byte[]? rented = null;
        var actual = expected.Length <= _stackLimit
            ? stackalloc byte[expected.Length]
            : (rented = ArrayPool<byte>.Shared.Rent(expected.Length)).AsSpan(0, expected.Length);
        try
        {
            return Convert.TryFromBase64String(actualBase64, actual, out int written)
                && written == expected.Length
                && CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<byte>.Shared.Return(rented, clearArray: true);
            }
        }
    }
}
