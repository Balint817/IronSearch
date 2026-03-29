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
        static IEnumerable<string> GetStrings_Author(MusicInfo musicInfo)
        {

            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.author))
            {
                yield return item;
            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).author))
                {
                    yield return item;
                }
            }
        }
        internal static bool EvalAuthor(PeroString pStr, MusicInfo musicInfo, string value)
        {
            return GetStrings_Author(musicInfo).Any(x => x.LowerContains(value) || pStr.LowerContains(x, value));
        }

        internal static bool EvalAuthor(MusicInfo musicInfo, Regex value)
        {
            return GetStrings_Author(musicInfo).Any(x => value.IsMatch(x));
        }
        internal static bool EvalAuthor(MusicInfo musicInfo, FuzzyContains value)
        {
            return GetStrings_Author(musicInfo).Any(x => value.IsMatch(x));
        }

        internal static bool EvalAuthor(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);

            switch (varArgs[0])
            {
                case Regex re:
                    return EvalAuthor(M.I, re);
                case string s:
                    return EvalAuthor(M.PS, M.I, s);
                case FuzzyContains fc:
                    return EvalAuthor(M.I, fc);
            }

            throw new SearchWrongTypeException("a string or regular expression", varArgs[0]?.GetType(), "Author()");
        }
    }
}
