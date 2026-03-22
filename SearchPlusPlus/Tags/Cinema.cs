using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static HashSet<string> hasCinema { get; set; } = new();

        internal static DateTime lastCheckedCinema;

        internal static bool isCinemaModified;

        internal static bool EvalCinema(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalCinemaInternal(musicInfo);
        }
        internal static bool EvalCinemaInternal(MusicInfo musicInfo)
        {
            var customInfo = AlbumManager.LoadedAlbums.Values.First(x => x.Uid == musicInfo.uid);
            if (!customInfo.IsPackaged)
            {
                return Utils.TryParseCinemaJson(customInfo, false);
            }
            var lastModified = File.GetLastAccessTimeUtc(customInfo.Path);
            if (lastCheckedCinema >= lastModified)
            {
                return hasCinema.Contains(musicInfo.uid);
            }
            isCinemaModified = true;
            if (Utils.TryParseCinemaJson(customInfo))
            {
                hasCinema.Add(musicInfo.uid);
                return true;
            }
            ;
            hasCinema.Remove(musicInfo.uid);
            return false;
        }
        internal static bool EvalCinema(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            return EvalCinema(M.I);
        }
    }
}
