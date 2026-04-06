using IronSearch.Tags;
using PythonExpressionManager;
using System.Collections.ObjectModel;

namespace IronSearch
{
    public sealed class CustomTagInfo
    {
        internal readonly string[] _keys;

        public readonly BuiltInObjectDelegate Method;
        public readonly string HelpString;
        public readonly ReadOnlyCollection<string> Keys;
        public CustomTagInfo(BuiltInObjectDelegate del, IEnumerable<string> keys, string helpString)
        {
            ArgumentNullException.ThrowIfNull(del, nameof(del));
            ArgumentNullException.ThrowIfNull(keys, nameof(keys));

            _keys = keys.ToArray();
            if (_keys.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(keys), $"expected '{nameof(keys)}' to not be empty");
            }
            Keys = new(_keys);

            HelpString = helpString ?? string.Empty;
            Method = del;

        }
    }
}