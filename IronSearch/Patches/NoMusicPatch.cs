namespace IronSearch.Patches
{
    //[HarmonyPatch(typeof(LocalizationName), "GetMusicTagNoMusicTxt")]
    internal class NoMusicPatch
    {
        private static bool Prefix(ref string __result)
        {
            if (RefreshPatch.isAdvancedSearch == false)
            {
                return true;
            }

            if (RefreshPatch.isAdvancedSearch == true)
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
    //[HarmonyPatch(typeof(LocalizationName), "GetSearchNameTxt")]
    internal class SearchNamePatch
    {
        private static bool Prefix(ref string __result)
        {
            if (RefreshPatch.isAdvancedSearch != false)
            {
                __result = "Results";
                return false;
            }
            return true;
        }
    }
}
