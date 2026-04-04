using HarmonyLib;
using Il2Cpp;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(LocalizationName), "GetSearchNameTxt")]
    internal class Localization_SearchNamePatch
    {
        private static bool Prefix(ref string __result)
        {
            if (SearchResults_RefreshPatch.isAdvancedSearch != false)
            {
                __result = "Results";
                return false;
            }
            return true;
        }
    }
}
