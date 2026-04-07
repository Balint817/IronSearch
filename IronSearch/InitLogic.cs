using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using IronSearch.Loaders;
using IronSearch.Tags;
using IronSearch.Utils;
using MelonLoader;

namespace IronSearch
{
    internal class InitLogic
    {
        internal static void LoadCustomDict()
        {
            foreach (var album in AlbumManager.LoadedAlbums.Values)
            {
                ModMain.uidToCustom[album.Uid] = album;
            }
        }
        internal static bool IsFirstLengthCacheBuild { get; private set; } = true;
        internal static void BuildCacheIfNecessary()
        {
            if (ModMain.InitSuccessful && IsFirstLengthCacheBuild)
            {
                IsFirstLengthCacheBuild = false;
                if (LengthLoader.VanillaCache?.IsEmpty ?? true)
                {
                    var s = "Re-building length cache, this may take a while!";
                    MelonLogger.Msg(System.ConsoleColor.Magenta, s);
                }
                LengthLoader.ForceBuildVanillaCache();
            }
        }
        internal static void LoadAlbumNames()
        {
            var localAlbums = Singleton<ConfigManager>.instance.GetConfigObject<DBConfigAlbums>(0).m_LocalDic.Values;

            var baseAlbums = Singleton<ConfigManager>.instance.GetConfigObject<DBConfigAlbums>(0).list;

            var t = new Dictionary<int, HashSet<string>>();

            foreach (var localAlbum in localAlbums)
            {
                for (int i = 0; i < baseAlbums.Count; i++)
                {
                    if (!t.TryGetValue(baseAlbums[i].albumUidIndex, out var l))
                    {
                        t[baseAlbums[i].albumUidIndex] = l = new();
                        l.Add(baseAlbums[i].title);
                    }
                    l.Add(localAlbum.list[i]);
                }
            }

            BuiltIns.albumNameLists = t.ToDictionary(x => x.Key, x => x.Value.ToList());
        }
        internal static void LoadCinema()
        {
            try
            {
                MelonLogger.Msg("Checking charts for cinemas, this shouldn't take long...");
                BuiltIns.hasCinema = AlbumManager.LoadedAlbums.Values.Where(x => MapUtils.TryParseCinemaJson(x)).Select(x => x.Uid).ToHashSet();
                MelonLogger.Msg("Cinema tag initialized");
            }
            catch (Exception ex)
            {

                MelonLogger.Msg(System.ConsoleColor.Red, ex.ToString());
                MelonLogger.Msg(System.ConsoleColor.Yellow, "If you're seeing this, then I have absolutely 0 clue how. Either way, the cinema tag won't work. (e.g. please report lmao)");
            }
        }
    }
}
