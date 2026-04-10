using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels.PnlMusicTag;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(PnlMusicSearchItem), "OnTextChanged")]
    internal class PnlMusic_TextChangedPatch
    {
        private static void Postfix(PnlMusicSearchItem __instance)
        {
            ModMain.SearchManager?.AutoComplete?.StopCurrentAutoComplete();
        }
    }
}
