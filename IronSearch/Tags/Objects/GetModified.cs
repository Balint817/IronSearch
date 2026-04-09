using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static double? GetModified(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo))
            {
                return null;
            }
            return DateTime.Now.Subtract(GetModifiedInternal(musicInfo)).TotalSeconds;
        }
        internal static DateTime GetModifiedInternal(MusicInfo musicInfo)
        {
            var album = (Album)ModMain.uidToCustom[musicInfo.uid];
            return File.GetLastWriteTimeUtc(album.Path);
        }
        internal static dynamic EvalGetModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "GetModified", varArgs, varKwargs);
            ThrowIfNotEmpty(varArgs, "GetModified", varArgs, varKwargs);
            return GetModified(M.I)!;
        }
    }
}
