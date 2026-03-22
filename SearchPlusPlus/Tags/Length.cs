using System.Collections.Concurrent;
using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using MelonLoader;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalLength(MusicInfo musicInfo, string value)
        {
            if (!Utils.ParseRange(value, out var range))
            {
                range = null;
                var split = value.Split(':').Reverse().ToArray();
                var sum = 0.0;
                if (split.Length is 2 or 3)
                {
                    if (!Utils.TryParseUInt(split[0], out var t) || t >= 60)
                    {
                        throw new SearchInputException($"failed to evaluate \"{value}\" as a range");
                    }
                    sum += t;
                    if (!Utils.TryParseUInt(split[1], out t) || t >= 60)
                    {
                        throw new SearchInputException($"failed to evaluate \"{value}\" as a range");
                    }
                    sum += t * 60;
                }
                if (split.Length == 3)
                {
                    if (!Utils.TryParseUInt(split[2], out var t) || t >= 60)
                    {
                        throw new SearchInputException($"failed to evaluate \"{value}\" as a range");
                    }
                    sum += t * 60 * 60;
                }
                if (sum == 0)
                {
                    throw new SearchInputException($"failed to evaluate \"{value}\" as a range");
                }
                range = new Range(sum - 0.5, sum + 0.5) { ExclusiveEnd = true };
            }
            return EvalLength(musicInfo, range);
        }
        internal static bool EvalLength(MusicInfo musicInfo, Range value)
        {
            return EvalLength(musicInfo, value.AsMultiRange());
        }
        internal static bool EvalLength(MusicInfo musicInfo, MultiRange value)
        {
            var length = AudioHelper.GetMusicLength(musicInfo);
            if (value == MultiRange.InvalidRange)
            {
                return length is null;
            }
            if (length is not { } ts)
            {
                return false;
            }

            return value.Contains(ts.TotalSeconds);
        }

        internal static bool EvalLength(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case string n:
                    return EvalLength(M.I, n);
                case Range r:
                    return EvalLength(M.I, r);
                case MultiRange mr:
                    return EvalLength(M.I, mr);
            }
            throw new SearchInputException("expected range string, or range, as length");
        }
    }
}
