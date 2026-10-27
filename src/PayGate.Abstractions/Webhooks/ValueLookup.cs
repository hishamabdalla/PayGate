namespace PayGate.Webhooks;

/// <summary>Case-insensitive reads of header and query collections, whatever comparer the caller's dictionary uses.</summary>
internal static class ValueLookup
{
    public static string? First(IReadOnlyDictionary<string, IReadOnlyList<string>> values, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (values.TryGetValue(name, out var exact))
        {
            return exact.Count > 0 ? exact[0] : null;
        }

        foreach (var (key, list) in values)
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                return list.Count > 0 ? list[0] : null;
            }
        }

        return null;
    }
}
