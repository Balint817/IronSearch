using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalAuthor(PeroString pStr, MusicInfo musicInfo, string value)
        {
            if (pStr.LowerContains(musicInfo.author ?? "", value))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (pStr.LowerContains(musicInfo.GetLocal(i).author ?? "", value))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool EvalAuthor(MusicInfo musicInfo, Regex re)
        {
            if (re.IsMatch(musicInfo.author ?? ""))
            {
                return true;
            }

            for (int i = 1; i <= 5; i++)
            {
                if (re.IsMatch(musicInfo.GetLocal(i).author ?? ""))
                {
                    return true;
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
                default:
                    break;
            }

            throw new SearchInputException("expected string or regex as author");
        }
    }
}
