using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHasHidden(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            var musicDiff = M.I.GetMusicLevelStringByDiff(4, false);
            if (string.IsNullOrEmpty(musicDiff) || musicDiff == "0" || (EvalCustom(M.I) && EvalHasHiddenCustom(M.I)))
            {
                return false;
            }
            return true;
        }

        internal static bool EvalHasHiddenCustom(MusicInfo musicInfo)
        {
            return !AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets.ContainsKey(4);
        }
    }
}
