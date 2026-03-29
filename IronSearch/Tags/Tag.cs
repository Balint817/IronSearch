using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalTag(PeroString pStr, MusicInfo musicInfo, string value)
        {
            var uidToInfo = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigMusicSearchTag>(0).m_Dictionary;

            if (uidToInfo.ContainsKey(musicInfo.uid))
            {
                var tags = uidToInfo[musicInfo.uid]?.tag;
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        foreach (var item in RomanizationHelper.GetAllRomanizations(tag))
                        {
                            if (pStr.LowerContains(item, value))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal static bool EvalTag(MusicInfo musicInfo, Regex re)
        {
            var uidToInfo = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigMusicSearchTag>(0).m_Dictionary;

            if (uidToInfo.ContainsKey(musicInfo.uid))
            {
                var tags = uidToInfo[musicInfo.uid]?.tag;
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {

                        foreach (var item in RomanizationHelper.GetAllRomanizations(tag))
                        {
                            if (re.IsMatch(item))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        internal static bool EvalTag(MusicInfo musicInfo, FuzzyContains fc)
        {
            var uidToInfo = Singleton<ConfigManager>.instance
                .GetConfigObject<DBConfigMusicSearchTag>(0).m_Dictionary;

            if (uidToInfo.ContainsKey(musicInfo.uid))
            {
                var tags = uidToInfo[musicInfo.uid]?.tag;
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {

                        foreach (var item in RomanizationHelper.GetAllRomanizations(tag))
                        {
                            if (fc.IsMatch(item))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
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
