using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHasTouhou(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            var musicDiff = M.I.GetMusicLevelStringByDiff(5, false);
            if (string.IsNullOrEmpty(musicDiff) || musicDiff == "0" || (EvalCustom(M.I) && EvalHasTouhouCustom(M.I)))
            {
                return false;
            }
            return true;
        }

        internal static bool EvalHasTouhouCustom(MusicInfo musicInfo)
        {
            return !AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid).Sheets.ContainsKey(5);
        }
    }
}
