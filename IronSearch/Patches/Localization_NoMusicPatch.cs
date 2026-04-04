using HarmonyLib;
using Il2Cpp;
using IronSearch.Core;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(LocalizationName), "GetMusicTagNoMusicTxt")]
    internal class Localization_NoMusicPatch
    {
        private static bool Prefix(ref string __result)
        {
            if (ActiveSearch.isAdvancedSearch == false)
            {
                return true;
            }

            if (ActiveSearch.isAdvancedSearch == true)
            {
                __result = "But nobody came.";
            }
            else
            {
                __result = "Error; Check your console";
            }
            return false;
        }
    }
}
