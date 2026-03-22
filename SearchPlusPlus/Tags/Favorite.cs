using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalFavorite(MusicInfo musicInfo)
        {
            return RefreshPatch.favorites.Contains(musicInfo.uid);
        }
        internal static bool EvalFavorite(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            return EvalFavorite(M.I);
        }
    }
}
