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
        internal static bool EvalOnline(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo)) return false;
            return EvalOnlineInternal(musicInfo);
        }

        private static bool EvalOnlineInternal(MusicInfo musicInfo)
        {
            return AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets.Values.Any(x => ModMain._hqChartDict.ContainsKey(x.Md5));
        }

        internal static bool EvalOnline(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            return EvalOnline(M.I);
        }
    }
}
