using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalAuthor(PeroString pStr, MusicInfo musicInfo, string value)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.author))
            {
                if (pStr.LowerContains(item, value))
                {
                    return true;
                }

            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).author))
                {
                    if (pStr.LowerContains(item, value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool EvalAuthor(MusicInfo musicInfo, Regex re)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.author))
            {
                if (re.IsMatch(item))
                {
                    return true;
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).author))
                {
                    if (re.IsMatch(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        internal static bool EvalAuthor(MusicInfo musicInfo, FuzzyContains fc)
        {
            foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.author))
            {
                if (fc.IsMatch(item))
                {
                    return true;
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(musicInfo.GetLocal(i).author))
                {
                     if (fc.IsMatch(item))
                    {
                        return true;
                    }
                }
            }

            return false;
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
