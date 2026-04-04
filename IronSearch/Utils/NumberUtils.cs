using System.Globalization;

namespace IronSearch.Utils
{
    public static class NumberUtils
    {
        public const NumberStyles numberStyles = NumberStyles.Number
            ^ NumberStyles.AllowThousands
            ^ NumberStyles.AllowTrailingSign; // why the fuck is this a thing?????????????
        public static bool TryParseDouble(this string s, out double x)
        {
            return double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseFloat(this string s, out float x)
        {
            return float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this string s, out long x)
        {
            return long.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this string s, out int x)
        {
            return int.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this string s, out uint x)
        {
            return uint.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }

        public static bool TryParseDouble(this ReadOnlySpan<char> s, out double x)
        {
            return double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseFloat(this ReadOnlySpan<char> s, out float x)
        {
            return float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this ReadOnlySpan<char> s, out long x)
        {
            return long.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this ReadOnlySpan<char> s, out int x)
        {
            return int.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this ReadOnlySpan<char> s, out uint x)
        {
            return uint.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }

        public static bool TryParseDouble(this Span<char> s, out double x)
        {
            return double.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseFloat(this Span<char> s, out float x)
        {
            return float.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseLong(this Span<char> s, out long x)
        {
            return long.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseInt(this Span<char> s, out int x)
        {
            return int.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
        public static bool TryParseUInt(this Span<char> s, out uint x)
        {
            return uint.TryParse(s, numberStyles, CultureInfo.InvariantCulture, out x);
        }
    }
}
