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
        internal static bool EvalCustom(MusicInfo musicInfo)
        {
            if (!ModMain.CustomAlbumsLoaded)
            {
                return false;
            }
            return EvalCustomInternal(musicInfo);
        }
        internal static bool EvalCustomInternal(MusicInfo musicInfo)
        {
            return AlbumManager.LoadedAlbums.Values.Any(x => x.Uid == musicInfo.uid);
        }
        internal static bool EvalCustom(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            return EvalCustom(M.I);
        }
    }
}
