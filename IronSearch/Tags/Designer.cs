using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;
using System.Text.RegularExpressions;
using static IronPython.Modules._ast;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static IEnumerable<string> GetStrings_Designer(MusicInfo musicInfo)
        {

            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.levelDesigner))
            {
                yield return item;
            }

            Utils.GetAvailableMaps(musicInfo, out var availableMaps);
            foreach (var i in availableMaps)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLevelDesignerStringByIndex(i)))
                {
                    yield return item;
                }
            }
        }
        internal static bool EvalDesigner(PeroString pStr, MusicInfo musicInfo, string value)
        {
            return GetStrings_Designer(musicInfo).Any(x => x.LowerContains(value) || pStr.LowerContains(x, value));
        }

        internal static bool EvalDesigner(MusicInfo musicInfo, Regex value)
        {
            return GetStrings_Designer(musicInfo).Any(x => value.IsMatch(x));
        }

        internal static bool EvalDesigner(MusicInfo musicInfo, FuzzyContains value)
        {
            return GetStrings_Designer(musicInfo).Any(x => value.IsMatch(x));
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
