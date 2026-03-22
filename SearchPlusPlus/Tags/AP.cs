using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalAPArgCount = new(0, 1);
        static readonly int[] evalAPDiffs = new[] { 1, 2, 3, 4 };

        internal static bool EvalAP(MusicInfo musicInfo, string value)
        {
            value = value.Trim(' ');

            Range diffRange;

            if (!Utils.ParseRange(value, out diffRange))
            {
                throw new SearchInputException($"failed to parse range \"{value}\"");
            }

            if (diffRange == Range.InvalidRange)
            {
                // wildcard → highest diff only
                if (!Utils.GetAvailableMaps(musicInfo, out var maps) || !maps.Intersect(evalAPDiffs).Any())
                    return false;

                var max = maps.Intersect(evalAPDiffs).Max();
                return EvalAP(musicInfo, new Range(max, max));
            }

            return EvalAP(musicInfo, diffRange);
        }

        internal static bool EvalAP(MusicInfo musicInfo)
        {
            return EvalAP(musicInfo, MultiRange.InvalidRange);
        }

        internal static bool EvalAP(MusicInfo musicInfo, Range diffRange)
        {
            return EvalAP(musicInfo, diffRange.AsMultiRange());
        }

        internal static bool EvalAP(MusicInfo musicInfo, MultiRange diffRange)
        {
            if (!Utils.GetAvailableMaps(musicInfo, out var availableMaps))
            {
                return false;
            }

            if (diffRange != MultiRange.InvalidRange)
            {
                availableMaps = availableMaps.Where(x => diffRange.Contains(x)).ToHashSet();
            }
            else
            {
                if (!availableMaps.Intersect(evalAPDiffs).Any())
                {
                    return false;
                }
                availableMaps = new HashSet<int>() { availableMaps.Intersect(evalAPDiffs).Max() };
            }

            if (availableMaps.Count == 0)
            {
                return false;
            }

            foreach (var diff in availableMaps)
            {
                string s = musicInfo.uid + "_" + diff;

                if (!RefreshPatch.highScores.Any(x =>
                        x.Uid == s &&
                        x.Accuracy == 1.0f))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool EvalAP(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotInRange(varArgs, evalAPArgCount);

            switch (varArgs.Length)
            {
                case 0:
                    return EvalAP(M.I);

                case 1:
                    switch (varArgs[0])
                    {
                        case string s:
                            return EvalAP(M.I, s);
                        case Range r:
                            return EvalAP(M.I, r);
                        case MultiRange mr:
                            return EvalAP(M.I, mr);
                    }
                    break;
            }

            throw new SearchInputException("invalid argument types for AP");
        }
    }
}
