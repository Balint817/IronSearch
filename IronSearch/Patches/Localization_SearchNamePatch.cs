using HarmonyLib;
using Il2Cpp;
using IronSearch.Core;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(LocalizationName), "GetSearchNameTxt")]
    internal class Localization_SearchNamePatch
    {
        private static bool Prefix(ref string __result)
        {
            if (ActiveSearch.isAdvancedSearch != false)
            {
                __result = "Results";
                return false;
            }
            return true;
        }
    }
}
