using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalCallback(MusicInfo musicInfo, int n)
        {
            return EvalCallback(musicInfo, new Range(n));
        }
        internal static bool EvalCallback(MusicInfo musicInfo, string s)
        {
            if (!Utils.ParseRange(s, out var r))
            {
                throw new SearchInputException($"failed to parse range '{s}'");
            }
            return EvalCallback(musicInfo, r.AsMultiRange());
        }
        internal static bool EvalCallback(MusicInfo musicInfo, Range r)
        {
            if (r == Range.InvalidRange)
            {
                throw new SearchInputException($"received invalid value");
            }
            return EvalCallback(musicInfo, r.AsMultiRange());
        }

        internal static bool EvalCallback(MusicInfo musicInfo, MultiRange mr)
        {
            if (mr == MultiRange.InvalidRange)
            {
                throw new SearchInputException($"received invalid value");
            }

            Utils.GetMapCallbacks(musicInfo, out var availableMaps);

            for (int i = 0; i < 4; i++)
            {
                var musicDiff = availableMaps[i];
                if (mr.Contains(musicDiff))
                {
                    return true;
                }
            }

            return false;
        }
        internal static bool EvalCallback(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);

            switch (varArgs[0])
            {
                case int n:
                    return EvalCallback(M.I, n);
                case string s:
                    return EvalCallback(M.I, s);
                case Range r:
                    return EvalCallback(M.I, r);
                case MultiRange mr:
                    return EvalCallback(M.I, mr);
                default:
                    break;
            }
            return false;
        }
    }
}
