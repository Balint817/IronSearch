using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels.PnlMusicTag;
using IronSearch.UI;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(PnlMusicSearchItem), "OnTextChanged")]
    internal class PnlMusic_TextChangedPatch
    {
        private static void Postfix(PnlMusicSearchItem __instance)
        {
            AutoCompleteManager.StopCurrentAutoComplete();
        }
    }
}
