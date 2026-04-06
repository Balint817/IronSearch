using HarmonyLib;
using Il2CppAssets.Scripts.Structs.Modules;
using IronSearch.Core;

namespace IronSearch.Patches
{


    [HarmonyPatch(typeof(SearchResults), "RefreshData")]
    internal static class SearchResults_RefreshPatch
    {
        //Singleton<TerminalManager>
        //DBMusicTagDefine.newMusicUids;
        internal static void Postfix()
        {
            ActiveSearch.isAdvancedSearch = false;
        }
        internal static bool Prefix(SearchResults __instance, string keyword)
        {
            ActiveSearch.isAdvancedSearch = false;

            switch (ActiveSearch.Run(keyword, out var results))
            {
                case SearchResult.OK:
                    break;
                case SearchResult.Error:
                    return false;
                case SearchResult.Vanilla:
                    return true;
                default:
                    throw new InvalidOperationException("how tf did you get here? (invalid SearchResult value)");
            }

            __instance.musicResult.m_Unlock.Clear();
            __instance.musicResult.m_Lock.Clear();
            __instance.authorResult.m_Unlock.Clear();
            __instance.authorResult.m_Lock.Clear();
            __instance.levelDesignerResult.m_Unlock.Clear();
            __instance.levelDesignerResult.m_Lock.Clear();
            __instance.musicAlbumResults.Clear();

            foreach (var musicInfo in results)
            {
                __instance.musicResult.m_Unlock.Add(musicInfo);
            }

            ModMain.Config.SearchHistoryMutable.Add(keyword);
            ModMain.Config.SearchHistoryMutable.RemoveAt(0);

            return false;
        }

    }
}