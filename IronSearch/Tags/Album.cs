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
        internal static Dictionary<int, List<string>> albumNameLists { get; set; } = null!;

        static IEnumerable<string> GetStrings_Album(MusicInfo musicInfo)
        {
            if (BuiltIns.albumNameLists.TryGetValue(musicInfo.m_MusicExInfo.m_AlbumUidIndex, out var albumNames))
            {
                foreach (var item in RomanizationHelper.GetAllRomanizations(albumNames))
                {
                    yield return item;
                }
            }
        }
        internal static bool EvalAlbum(MusicInfo musicInfo, PeroString pStr, string value)
        {
            return GetStrings_Album(musicInfo).Any(x => x.LowerContains(value) || pStr.LowerContains(x, value));
        }
        internal static bool EvalAlbum(MusicInfo musicInfo, Regex value)
        {
            return GetStrings_Album(musicInfo).Any(x => value.IsMatch(x));
        }
        internal static bool EvalAlbum(MusicInfo musicInfo, FuzzyContains value)
        {
            return GetStrings_Album(musicInfo).Any(x => value.IsMatch(x));
        }

        internal static bool EvalAlbum(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);
            switch (varArgs[0])
            {
                case string s:
                    return EvalAlbum(M.I, M.PS, s);
                case Regex r:
                    return EvalAlbum(M.I, r);
                case FuzzyContains fc:
                    return EvalAlbum(M.I, fc);
            }
            throw new SearchWrongTypeException("a string or regular expression", varArgs[0]?.GetType(), "Album()");
        }
    }
}
