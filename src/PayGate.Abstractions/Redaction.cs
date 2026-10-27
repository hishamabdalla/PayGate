namespace PayGate;

/// <summary>Masks sensitive values in <c>ToString()</c> output.</summary>
internal static class Redaction
{
    public const string Masked = "***";

    public static string? Mask(string? value) => value is null ? null : Masked;
}
