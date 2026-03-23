using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Patches;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly int[] evalUnplayedDiffs = new[] { 1, 2, 3, 4 };
        static readonly Range evalUnplayedArgCount = new Range(0, 1);
        internal static bool EvalUnplayed(MusicInfo musicInfo)
        {
            string s = musicInfo.uid + "_";

            return RefreshPatch.highScores.FindIndex(x => x.Uid.StartsWith(s)) == -1;
        }


        internal static bool EvalUnplayed(MusicInfo musicInfo, int value)
        {
            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            if (!availableMaps.Contains(value))
            {
                return false;
            }
            return IsUnplayed(musicInfo, value);
        }
        internal static bool EvalUnplayed(MusicInfo musicInfo, string value)
        {
            if (!Utils.ParseRange(value, out var range))
            {
                throw new SearchInputException($"failed to parse range '{value}'");
            }
            return EvalUnplayed(musicInfo, range.AsMultiRange());
        }
        internal static bool EvalUnplayed(MusicInfo musicInfo, Range value)
        {
            return EvalUnplayed(musicInfo, value.AsMultiRange());
        }
        internal static bool EvalUnplayed(MusicInfo musicInfo, MultiRange value)
        {
            if (!Utils.GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }
            if (value != MultiRange.InvalidRange)
            {
                var t = availableMaps.Where((int x) => value.Contains(x)).ToArray();
                if (!t.Any())
                {
                    return false;
                }
                return t.All(x => IsUnplayed(musicInfo, x));
            }
            if (!availableMaps.Intersect(evalUnplayedDiffs).Any())
            {
                return false;
            }
            return IsUnplayed(musicInfo, availableMaps.Intersect(evalUnplayedDiffs).Max());
        }
        internal static bool IsUnplayed(MusicInfo musicInfo, int diff)
        {
            string s = musicInfo.uid + "_" + diff;
            return RefreshPatch.highScores.FindIndex(x => x.Uid == s) == -1;
        }

        internal static bool EvalUnplayed(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, evalUnplayedArgCount);
            if (varArgs.Length == 0)
            {
                return EvalUnplayed(M.I);
            }
            switch (varArgs[0])
            {
                case int n:
                    return EvalUnplayed(M.I, n);
                case string s:
                    return EvalUnplayed(M.I, s);
                case Range r:
                    return EvalUnplayed(M.I, r);
                case PythonRange pr:
                    return EvalUnplayed(M.I, (Range)pr);
                case MultiRange mr:
                    return EvalUnplayed(M.I, mr);
                default:
                    break;
            }
            return false;
        }
    }
}
