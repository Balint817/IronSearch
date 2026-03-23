using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Patches;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly int[] evalFCDiffs = new[] { 1, 2, 3, 4 };
        static readonly Range evalFCArgCount = new(0, 1);



        internal static bool EvalFC(MusicInfo musicInfo, int value)
        {
            return EvalFC(musicInfo, value);
        }
        internal static bool EvalFC(MusicInfo musicInfo, string value)
        {
            if (!Utils.ParseRange(value, out var range))
            {
                throw new SearchInputException($"failed to parse range '{value}'");
            }
            return EvalFC(musicInfo, range.AsMultiRange());
        }
        internal static bool EvalFC(MusicInfo musicInfo, Range value)
        {
            return EvalFC(musicInfo, value.AsMultiRange());
        }

        internal static bool EvalFC(MusicInfo musicInfo, MultiRange value)
        {
            if (!Utils.GetAvailableMaps(musicInfo, out var availableMaps, out var isCustom))
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
                return t.All(x => RefreshPatch.fullCombos.Contains(musicInfo.uid + "_" + x));
            }

            if (!availableMaps.Intersect(evalFCDiffs).Any())
            {
                return false;
            }
            return RefreshPatch.fullCombos.Contains(musicInfo.uid + "_" + availableMaps.Intersect(evalFCDiffs).Max());

        }
        internal static bool EvalFC(MusicInfo musicInfo)
        {
            if (!Utils.GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            string s = musicInfo.uid + "_";
            foreach (var diff in availableMaps)
            {
                var t = s + diff;
                if (!RefreshPatch.fullCombos.Contains(t))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool EvalFC(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, evalFCArgCount);
            if (varArgs.Length == 0)
            {
                return EvalFC(M.I);
            }
            switch (varArgs[0])
            {
                case int n:
                    return EvalFC(M.I, n);
                case string s:
                    return EvalFC(M.I, s);
                case Range r:
                    return EvalFC(M.I, r);
                case PythonRange pr:
                    return EvalFC(M.I, (Range)pr);
                case MultiRange mr:
                    return EvalFC(M.I, mr);
                default:
                    break;
            }
            return false;
        }
    }
}
