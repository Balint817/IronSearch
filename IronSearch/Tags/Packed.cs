using CustomAlbums.Data;
using CustomAlbums.Managers;
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
            ThrowIfNotEmpty(varArgs, "Packed()");
            ThrowIfNotEmpty(varKwargs, "Packed()");
            return EvalPacked(M.I);
        }
    }
}
