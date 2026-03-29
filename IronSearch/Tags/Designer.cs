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
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.levelDesigner))
            {
                if (pStr.LowerContains(item, value))
                {
                    return true;
                }
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLevelDesignerStringByIndex(i)))
                {
                    if (pStr.LowerContains(item, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool EvalDesigner(MusicInfo musicInfo, Regex re)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.levelDesigner))
            {
                if (re.IsMatch(item))
                {
                    return true;
                }
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLevelDesignerStringByIndex(i)))
                {
                    if (re.IsMatch(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool EvalDesigner(MusicInfo musicInfo, FuzzyContains fc)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.levelDesigner))
            {
                if (fc.IsMatch(item))
                {
                    return true;
                }
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLevelDesignerStringByIndex(i) ?? ""))
                {
                     if (fc.IsMatch(item))
                    {
                        return true;
                    }
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
