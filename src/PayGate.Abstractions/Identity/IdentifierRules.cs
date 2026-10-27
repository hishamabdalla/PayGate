namespace PayGate;

/// <summary>Shared validation for provider ids and account names.</summary>
internal static class IdentifierRules
{
    public static string Normalize(string value, bool allowUnderscore, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);

        string trimmed = value.Trim();
        return string.Create(trimmed.Length, (trimmed, allowUnderscore, paramName), static (span, state) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                char c = state.trimmed[i];
                char lower = c is >= 'A' and <= 'Z' ? (char)(c + ('a' - 'A')) : c;
                bool valid = lower is >= 'a' and <= 'z' or >= '0' and <= '9' or '-'
                    || (state.allowUnderscore && lower == '_');
                if (!valid)
                {
                    throw new ArgumentException(
                        $"'{state.trimmed}' is not a valid identifier: only letters, digits and '-'{(state.allowUnderscore ? " or '_'" : string.Empty)} are allowed.",
                        state.paramName);
                }

                span[i] = lower;
            }
        });
    }
}
