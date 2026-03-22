using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalRanked(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo)) return false;
            return EvalRankedInternal(musicInfo);
        }

        private static bool EvalRankedInternal(MusicInfo musicInfo)
        {
            return AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets.Values.Any(x => ModMain._hqChartDict.ContainsKey(x.Md5) && ModMain._hqChartDict[x.Md5]);
        }

        internal static bool EvalRanked(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            return EvalRanked(M.I);
        }
    }
}
