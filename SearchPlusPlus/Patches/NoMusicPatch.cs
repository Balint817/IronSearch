using HarmonyLib;
using Il2Cpp;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(LocalizationName), "GetMusicTagNoMusicTxt")]
    internal class NoMusicPatch
    {
        static bool Prefix(ref string __result)
        {
            if (SearchPatch.isAdvancedSearch == true)
            {
                if (SearchPatch.searchError != null)
                {
                    __result = "Error; Check your console";
                }
                else
                {
                    __result = "But nobody came.";
                }
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(LocalizationName), "GetSearchNameTxt")]
    internal class SearchNamePatch
    {
        static bool Prefix(ref string __result)
        {
            if (SearchPatch.isAdvancedSearch == true)
            {
                __result = "Results";
                return false;
            }
            return true;
        }
    }
}
