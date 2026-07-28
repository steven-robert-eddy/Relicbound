using System;

namespace Relicbound.Content;

/// <remarks>
/// Matches content strings against enum members case-insensitively and
/// ignoring underscores, so JSON can stay UPPER_SNAKE (matching
/// docs/GAME_DESIGN.md's examples) while C# enums stay PascalCase. One
/// generic parser means adding a new Tag, EventType, or StatusType member
/// never needs a matching case here.
/// </remarks>
public static class ContentEnumParsing
{
    public static bool TryParse<TEnum>(string? raw, out TEnum value) where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(raw))
        {
            var normalized = raw.Replace("_", string.Empty, StringComparison.Ordinal);

            foreach (var candidate in Enum.GetValues<TEnum>())
            {
                if (string.Equals(candidate.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    value = candidate;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }
}
