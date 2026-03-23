using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static readonly Range evalDiffRange = new(1, 2);
        internal static bool EvalDifficulty(MusicInfo musicInfo, int n)
        {
            return EvalDifficulty(musicInfo, new Range(n));
        }
        internal static bool EvalDifficulty(MusicInfo musicInfo, string s)
        {
            if (!Utils.ParseRange(s, out var r))
            {
                throw new SearchInputException($"failed to parse range '{s}'");
            }
            return EvalDifficulty(musicInfo, r.AsMultiRange());
        }
        internal static bool EvalDifficulty(MusicInfo musicInfo, Range r)
        {
            return EvalDifficulty(musicInfo, r.AsMultiRange());
        }

        internal static bool EvalDifficulty(MusicInfo musicInfo, MultiRange mr)
        {
            return EvalDifficulty(musicInfo, mr, MultiRange.FullRange);
        }
        internal static bool EvalDifficulty(MusicInfo musicInfo, MultiRange diffRange, MultiRange levelRange)
        {
            bool diffIncludeString = false;

            if (diffRange == MultiRange.InvalidRange)
            {
                diffIncludeString = true;
            }
            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            Utils.GetMapDifficulties(musicInfo, out var difficulties);
            if (!availableMaps.Any())
            {
                return false;
            }

            if (levelRange == MultiRange.InvalidRange)
            {
                availableMaps = new() { availableMaps.Max() };
            }
            else
            {
                availableMaps = availableMaps.Where(x => levelRange.Contains(x)).ToHashSet();
            }

            foreach (var i in availableMaps)
            {
                var musicDiff = difficulties[i];
                if (!musicDiff.TryParseInt(out int x))
                {
                    if (diffIncludeString)
                    {
                        return true;
                    };

                }
                else if (diffRange.Contains(x))
                {
                    return true;
                }
            }

            return false;
        }
        internal static bool EvalDifficulty(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, evalDiffRange);

            if (varArgs.Length == 1)
            {
                switch (varArgs[0])
                {
                    case int n:
                        return EvalDifficulty(M.I, n);
                    case string s:
                        return EvalDifficulty(M.I, s);
                    case Range r:
                        return EvalDifficulty(M.I, r);
                    case PythonRange pr:
                        return EvalDifficulty(M.I, (Range)pr);
                    case MultiRange mr:
                        return EvalDifficulty(M.I, mr);
                    default:
                        throw new SearchInputException("invalid 'difficulty' argument");
                }
            }
            if (varArgs.Length == 2)
            {
                MultiRange diffRange;
                switch (varArgs[0])
                {
                    case int n:
                        diffRange = new Range(n).AsMultiRange();
                        break;
                    case string s:
                        if (!Utils.ParseRange(s, out var parsedRange))
                        {
                            throw new SearchInputException($"failed to parse range '{s}'");
                        }
                        diffRange = parsedRange.AsMultiRange();
                        break;
                    case Range r:
                        diffRange = r.AsMultiRange();
                        break;
                    case PythonRange pr:
                        diffRange = ((Range)pr).AsMultiRange();
                        break;
                    case MultiRange mr:
                        diffRange = mr;
                        break;
                    default:
                        throw new SearchInputException("invalid 'difficulty' argument");
                }
                MultiRange levelRange;
                switch (varArgs[1])
                {
                    case int n:
                        levelRange = new Range(n).AsMultiRange();
                        break;
                    case string s:
                        if (!Utils.ParseRange(s, out var parsedRange))
                        {
                            throw new SearchInputException($"failed to parse range '{s}'");
                        }
                        levelRange = parsedRange.AsMultiRange();
                        break;
                    case Range r:
                        levelRange = r.AsMultiRange();
                        break;
                    case PythonRange pr:
                        levelRange = ((Range)pr).AsMultiRange();
                        break;
                    case MultiRange mr:
                        levelRange = mr;
                        break;
                    default:
                        throw new SearchInputException("invalid 'difficulty' argument");
                }
                return EvalDifficulty(M.I, diffRange, levelRange);
            }

            throw new SearchInputException("how tf did u get here");
        }
    }
}
