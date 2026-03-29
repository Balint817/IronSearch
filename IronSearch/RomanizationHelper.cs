using Romanization;
using System.Collections.ObjectModel;

namespace IronSearch
{
    public static class RomanizationHelper
    {
        static readonly Dictionary<string, ReadOnlyCollection<string>> _cache = new();
        public static ReadOnlyCollection<string> GetAllRomanizations(string input)
        {
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
