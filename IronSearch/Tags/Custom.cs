using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

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
            ThrowIfNotEmpty(varArgs, "Custom()");
            ThrowIfNotEmpty(varKwargs, "Custom()");
            return EvalCustom(M.I);
        }
    }
}
