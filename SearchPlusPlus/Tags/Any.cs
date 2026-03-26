using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalAny(PeroString pStr, MusicInfo musicInfo, string filter)
        {
            return (EvalTag(pStr, musicInfo, filter) || EvalTitle(pStr, musicInfo, filter) || EvalAuthor(pStr, musicInfo, filter) || EvalDesigner(pStr, musicInfo, filter) || EvalAlbum(musicInfo, pStr, filter));
        }
        internal static bool EvalAny(MusicInfo musicInfo, Regex re)
        {
            return (EvalTag(musicInfo, re) || EvalTitle(musicInfo, re) || EvalAuthor(musicInfo, re) || EvalDesigner(musicInfo, re) || EvalAlbum(musicInfo, re));
        }
        internal static bool EvalAny(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);
            switch (varArgs[0])
            {
                case string s:
                    return EvalAny(M.PS, M.I, s);
                case Regex re:
                    return EvalAny(M.I, re);
                default:
                    break;
            }
            throw new SearchWrongTypeException("a string or regular expression for the combined search", varArgs[0]?.GetType(), "Any()");
        }
    }
}
