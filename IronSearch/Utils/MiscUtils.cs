using System.Diagnostics;

namespace IronSearch.Utils
{

    public static class MiscUtils
    {
        public static bool IsAssemblyLoaded(string shortName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == shortName) != null;
        }
        internal static bool LowerContains(this string compareText, string containsText)
        {
            return (compareText ?? "").ToLowerInvariant().Contains((containsText ?? "").ToLowerInvariant());
        }
        public static string GetFullStackTrace()
        {
            return new StackTrace(true).ToString();
        }

        public static TItem? MaxByOrDefault<TItem, TKey>(this IEnumerable<TItem> values, Func<TItem, TKey> transformer, TItem? defaultValue)
        {
            ArgumentNullException.ThrowIfNull(values, nameof(values));
            ArgumentNullException.ThrowIfNull(transformer, nameof(transformer));

            using var enumerator = values.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return defaultValue;
            }
            var maxValue = enumerator.Current;
            var maxKey = transformer(maxValue);
            var comparer = Comparer<TKey>.Default;
            while (enumerator.MoveNext())
            {
                var currentValue = enumerator.Current;
                var currentKey = transformer(currentValue);
                if (comparer.Compare(currentKey, maxKey) > 0)
                {
                    maxValue = currentValue;
                    maxKey = currentKey;
                }
            }
            return maxValue;
        }
    }
}
