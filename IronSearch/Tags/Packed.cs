using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalPacked(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo)) return false;
            return EvalPackedInternal(musicInfo);
        }

        private static bool EvalPackedInternal(MusicInfo musicInfo)
        {
            return ((Album)ModMain.uidToAlbum[musicInfo.uid]).IsPackaged;
        }

        internal static bool EvalPacked(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Packed", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Packed", varArgs, varKwargs);
            return EvalPacked(M.I);
        }
    }
}
