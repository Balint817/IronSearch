using HarmonyLib;
using IronSearch.Core;
using MelonLoader;
using Playlists.Patches;
using System.Reflection;

namespace IronSearch.Patches
{
    internal class Playlists_APIPatch
    {
        private static MethodInfo? _targetMethod;
        private static bool hasRun = false;
        private static void Prefix(string url)
        {
            if (hasRun)
            {
                return;
            }
            if (url != "musedash/v1/music_tag")
            {
                return;
            }
            hasRun = true;

            foreach (var cp in Playlists.Playlists.LoadedPlaylists)
            {
                if (cp is null || cp.Albums is null)
                {
                    continue;
                }
                var newAlbums = new List<string>(cp.Albums.Count);
                foreach (var item in cp.Albums.ToArray())
                {
                    if (string.IsNullOrEmpty(item) || !item.StartsWith(":", StringComparison.Ordinal))
                    {
                        newAlbums.Add(item);
                        continue;
                    }
                    var expression = ModMain.Config.StartString + item[1..];
                    ActiveSearch.SkipNextCall = false;
                    switch (ActiveSearch.Run(expression, out var result))
                    {
                        case SearchResult.OK:
                            newAlbums.AddRange(result.Select(x => x.uid));
                            break;
                        case SearchResult.Error:
                            MelonLogger.Msg(ConsoleColor.Red, $"Injection into the playlist '{cp.Name}' failed.");
                            break;
                        case SearchResult.Vanilla:
                            MelonLogger.Msg(ConsoleColor.Red, $"how tf did you get here? (SearchResult.Vanilla in Playlists)");
                            break;
                        default:
                            MelonLogger.Msg(ConsoleColor.Red, $"how tf did you get here? (SearchResult out of range in Playlists)");
                            break;
                    }
                }
                cp.Albums = newAlbums;
            }
        }
        internal static void RunPatch(HarmonyLib.Harmony harmonyInstance)
        {
            try
            {
                ;
                _targetMethod = AccessTools.Method(typeof(APIPatch), "Prefix");
                var prefix = Prefix;
                harmonyInstance.Patch(_targetMethod, prefix: new HarmonyMethod(prefix.Method));
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ConsoleColor.Red, ex);
                MelonLogger.Msg(ConsoleColor.Red, "Error occured while patching Playlists, Playlists integration features will not work.");
            }

        }
    }
}