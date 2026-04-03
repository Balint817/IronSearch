using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;

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
            var album = (Album)ModMain.uidToCustom[musicInfo.uid];
            return File.GetLastWriteTimeUtc(album.Path);
        }
        internal static dynamic EvalGetModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetModified", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "GetModified", varArgs, varKwargs);
            return GetModified(M.I)!;
        }
    }
}
