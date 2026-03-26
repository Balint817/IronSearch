using System.Collections.Concurrent;
using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Exceptions;
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
                if (!value.TryTimeStringToTicks(out var l))
                {
                    throw new SearchValidationException(
                        $"Could not parse \"{value}\" as a numeric range or a length/time string (for example mm:ss).",
                        "Length()");
                }
                var asSeconds = new TimeSpan(l).TotalSeconds;
                range = new Range(asSeconds - 0.5, asSeconds + 0.5) { ExclusiveEnd = true };
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
                case PythonRange pr:
                    return EvalLength(M.I, (Range)pr);
                case MultiRange mr:
                    return EvalLength(M.I, mr);
            }
            throw new SearchWrongTypeException("a range string, Python range, or multi-range object", varArgs[0]?.GetType(), "Length()");
        }
    }
}
