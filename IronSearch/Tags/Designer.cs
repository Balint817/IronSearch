using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalDesigner(PeroString pStr, MusicInfo musicInfo, string value)
        {
            if (pStr.LowerContains(musicInfo.levelDesigner ?? "", value))
            {
                return true;
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                if (pStr.LowerContains(musicInfo.GetLevelDesignerStringByIndex(i) ?? "", value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool EvalDesigner(MusicInfo musicInfo, Regex re)
        {
            if (re.IsMatch(musicInfo.levelDesigner ?? ""))
            {
                return true;
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                if (re.IsMatch(musicInfo.GetLevelDesignerStringByIndex(i) ?? ""))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool EvalDesigner(MusicInfo musicInfo, FuzzyContains fc)
        {
            if (fc.IsMatch(musicInfo.levelDesigner ?? ""))
            {
                return true;
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                if (fc.IsMatch(musicInfo.GetLevelDesignerStringByIndex(i) ?? ""))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool EvalDesigner(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);

            switch (varArgs[0])
            {
                case Regex re:
                    return EvalDesigner(M.I, re);
                case string s:
                    return EvalDesigner(M.PS, M.I, s);
                case FuzzyContains fc:
                    return EvalDesigner(M.I, fc);
            }

            throw new SearchWrongTypeException("a string or regular expression", varArgs[0]?.GetType(), "Designer()");
        }
    }
}
