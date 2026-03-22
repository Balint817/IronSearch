using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Records;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
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
            bool diffIncludeString = false;

            if (mr == MultiRange.InvalidRange)
            {
                diffIncludeString = true;
            }

            Utils.GetMapDifficulties(musicInfo, out var availableMaps);


            for (int i = 0; i < 4; i++)
            {
                var musicDiff = availableMaps[i];
                if (!musicDiff.TryParseInt(out int x))
                {
                    if (diffIncludeString)
                    {
                        return true;
                    }
                    ;

                }
                else if (mr.Contains(x))
                {
                    return true;
                }
            }

            return false;
        }
        internal static bool EvalDifficulty(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);

            switch (varArgs[0])
            {
                case int n:
                    return EvalDifficulty(M.I, n);
                case string s:
                    return EvalDifficulty(M.I, s);
                case Range r:
                    return EvalDifficulty(M.I, r);
                case MultiRange mr:
                    return EvalDifficulty(M.I, mr);
                default:
                    break;
            }
            return false;
        }
    }
}
