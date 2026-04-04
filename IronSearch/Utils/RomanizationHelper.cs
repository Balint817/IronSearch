using Romanization;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace IronSearch.Utils
{
    public static class RomanizationHelper
    {
        private static readonly ConcurrentDictionary<string, ReadOnlyCollection<string>> _cache = new()
        {
            [""] = new(new string[] { "" }),
        };
        public static ReadOnlyCollection<string> GetAllRomanizations(IEnumerable<string> input)
        {
            return new(input.SelectMany(x => GetAllRomanizations(x)).Distinct().ToArray());
        }
        private static readonly ReadOnlyCollection<string> _empty = new(Array.Empty<string>());

        private static readonly ThreadLocal<IRomanizationSystem[]> romanizationSystems = new(() =>
        {
            return new IRomanizationSystem[]
            {
                new Japanese.ModifiedHepburn(),
                new Japanese.KanjiReadings(),
                new Chinese.HanyuPinyin(),
                new Korean.RevisedRomanization(),
                new Korean.HanjaReadings(),
            };
        });
        public static ReadOnlyCollection<string> GetAllRomanizations(string input)
        {
            if (input is null)
            {
                return _empty;
            }
            if (_cache.TryGetValue(input, out var cached))
            {
                return cached;
            }

            var results = new HashSet<string>()
            {
                input
            };

            var systems = romanizationSystems.Value!;

            for (int i = 0; i < systems.Length; i++)
            {
                var system = systems[i];
                try
                {
                    results.Add(system.Process(input));
                }
                catch { }
            }

            cached = new(results.ToArray());
            _cache.TryAdd(input, cached);

            return cached;
        }
    }
}
