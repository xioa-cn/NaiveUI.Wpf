using System.Globalization;

namespace NaiveUI.NControls.Tools;

internal static class LengthValueParser
{
    public static double ResolveOrDefault(object? value, double fallback)
    {
        return TryParse(value, out var length) ? length : fallback;
    }

    public static bool TryParse(object? value, out double length)
    {
        switch (value)
        {
            case null:
                break;
            case double doubleValue when !double.IsNaN(doubleValue) && doubleValue > 0d:
                length = doubleValue;
                return true;
            case float floatValue when !float.IsNaN(floatValue) && floatValue > 0f:
                length = floatValue;
                return true;
            case int intValue when intValue > 0:
                length = intValue;
                return true;
            case long longValue when longValue > 0:
                length = longValue;
                return true;
            case string text:
            {
                var normalized = text.Trim();
                if (normalized.EndsWith("px", StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized[..^2].Trim();
                }

                if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedLength)
                    && parsedLength > 0d)
                {
                    length = parsedLength;
                    return true;
                }

                break;
            }
        }

        length = 0d;
        return false;
    }
}
