using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppPeroTools2.PeroString;
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
                        if (pStr.LowerContains(tag ?? "", value))
                        {
                            return true;
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
                        if (re.IsMatch(tag ?? ""))
                        {
                            return true;
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
                case string s:
                    return EvalTag(M.PS, M.I, s);
                default:
                    break;
            }

            throw new SearchInputException("expected string or regex as tag");
        }
    }
}
