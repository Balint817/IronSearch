using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppPeroPeroGames.GlobalDefines;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static DateTime? GetModified(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo))
            {
                return null;
            }
            return GetModifiedInternal(musicInfo);
        }
        internal static DateTime GetModifiedInternal(MusicInfo musicInfo)
        {
            var album = (Album)ModMain.uidToAlbum[musicInfo.uid];
            return File.GetLastWriteTimeUtc(album.Path);
        }
        internal static dynamic EvalGetModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetModified()");
            ThrowIfNotEmpty(varKwargs, "GetModified()");
            return GetModified(M.I)!;
        }
    }
}
