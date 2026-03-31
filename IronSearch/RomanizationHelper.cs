using System.Collections.ObjectModel;
using Romanization;

namespace IronSearch
{
    public static class RomanizationHelper
    {
        static readonly Dictionary<string, ReadOnlyCollection<string>> _cache = new()
        {
            [""] = new(new string[] { "" }),
        };
        public static ReadOnlyCollection<string> GetAllRomanizations(IEnumerable<string> input)
        {
            return new(input.SelectMany(x => GetAllRomanizations(x)).Distinct().ToArray());
        }
        static readonly ReadOnlyCollection<string> _empty = new(Array.Empty<string>());
        public static ReadOnlyCollection<string> GetAllRomanizations(string input)
        {
            if (input is null)
            {
                return _empty;
            }
            if (_cache.TryGetValue(input, out var cached))
                return cached;

            var results = new HashSet<string>();

            try
            {
                var processed = new Japanese.ModifiedHepburn();
                results.Add(processed.Process(input));
            }
            catch { }

            try
            {
                var processed = new Japanese.KanjiReadings();
                results.Add(processed.Process(input));
            }
            catch { }

            try
            {
                var processed = new Chinese.HanyuPinyin();
                results.Add(processed.Process(input));
            }
            catch { }

            try
            {
                var processed = new Korean.RevisedRomanization();
                results.Add(processed.Process(input));
            }
            catch { }

            try
            {
                var processed = new Korean.HanjaReadings();
                results.Add(processed.Process(input));
            }
            catch { }

            cached = new(results.ToArray());
            _cache.TryAdd(input, cached);

            return cached;
        }
    }
}
