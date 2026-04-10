using HarmonyLib;
using Il2CppAssets.Scripts.UI;
using Il2CppAssets.Scripts.UI.Panels.PnlMusicTag;
using IronSearch.UI;
using IronSearch.Utils;
using UnityEngine;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(PnlMusicSearchItem), "OnTextFocusChanged")]
    internal class PnlMusic_FocusChangedPatch
    {
        internal static PeroInputField? inputField;
        private static void Postfix(PnlMusicSearchItem __instance, bool focus)
        {
            if (focus)
            {
                inputField = __instance.m_InputField;
            }
            else
            {
                ModMain.SearchManager?.AutoComplete?.StopCurrentAutoComplete();
                inputField = null;
            }
        }

        internal static bool TryGetInputFieldPosition(out Vector2 position)
        {
            position = Vector2.zero;
            if (inputField is null)
            {
                return false;
            }
            position = inputField.GetCaretVectorPosition();
            return true;
        }
    }
}
