using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static HashSet<string> hasCinema { get; set; } = new();

        // TODO: rename to "lastCheckedCinema"
        internal static DateTime lastChecked;

        // TODO: rename to "isCinemaModified"
        internal static bool isModified;

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
            if (lastChecked >= lastModified)
            {
                return hasCinema.Contains(musicInfo.uid);
            }
            isModified = true;
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
