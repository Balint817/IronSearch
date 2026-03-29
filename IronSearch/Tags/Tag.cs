using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;
using System.Text.RegularExpressions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        static IEnumerable<string> GetStrings_Tag(MusicInfo musicInfo)
        {
            var uidToInfo = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigMusicSearchTag>(0).m_Dictionary;
            if (!uidToInfo.ContainsKey(musicInfo.uid))
            {
                yield break;
            }

            var tags = uidToInfo[musicInfo.uid]?.tag;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    foreach (var item in RomanizationHelper.GetAllRomanizations(tag))
                    {
                        yield return item;
                    }
                }
            }
        }
        internal static bool EvalTag(PeroString pStr, MusicInfo musicInfo, string value)
        {
            return GetStrings_Tag(musicInfo).Any(x => x.LowerContains(value) || pStr.LowerContains(x, value));
        }

        internal static bool EvalTag(MusicInfo musicInfo, Regex value)
        {
            return GetStrings_Tag(musicInfo).Any(x => value.IsMatch(x));
        }
        internal static bool EvalTag(MusicInfo musicInfo, FuzzyContains value)
        {
            return GetStrings_Tag(musicInfo).Any(x => value.IsMatch(x));
        }

        internal static bool EvalTag(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1);
            ThrowIfNotEmpty(varKwargs);

            switch (varArgs[0])
            {
                case Regex re:
                    return EvalTag(M.I, re);
                case FuzzyContains fc:
                    return EvalTag(M.I, fc);
                case string s:
                    return EvalTag(M.PS, M.I, s);
            }

            throw new SearchWrongTypeException("a string or regular expression", varArgs[0]?.GetType(), "Tag()");
        }
    }
}
