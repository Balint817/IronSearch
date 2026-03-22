using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static Dictionary<int, List<string>> albumNameLists { get; set; } = null!;
        internal static bool EvalAlbum(MusicInfo musicInfo, PeroString ps, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new SearchInputException("received empty value in 'album'");
            }
            if (!BuiltIns.albumNameLists.TryGetValue(musicInfo.m_MusicExInfo.m_AlbumUidIndex, out var albumNames))
            {
                return false;
            }
            return albumNames.Any(x => x.LowerContains(value) || ps.LowerContains(x, value));
        }

        internal static bool EvalAlbum(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);
            switch (varArgs[0])
            {
                case string s:
                    return EvalAlbum(M.I, M.PS, s);
                default:
                    break;
            }
            return false;
        }
    }
}
