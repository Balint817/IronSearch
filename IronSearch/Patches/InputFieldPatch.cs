using Il2CppAssets.Scripts.Structs.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels.PnlMusicTag;
using Il2CppAssets.Scripts.UI;
using MelonLoader;
using IronSearch.UI;
using UnityEngine.UI;
using UnityEngine;

namespace IronSearch.Patches
{
    [HarmonyPatch(typeof(PnlMusicSearchItem), "OnTextFocusChanged")]
    internal class SearchFocusPatch
    {
        internal static PeroInputField? inputField;
        static void Postfix(PnlMusicSearchItem __instance, bool focus)
        {
            if (focus)
            {
                inputField = __instance.m_InputField;
            }
            else
            {
                AutoCompleteManager.StopCurrentAutoComplete();
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
    [HarmonyPatch(typeof(PnlMusicSearchItem), "OnTextChanged")]
    internal class SearchTextChangedPatch
    {
        static void Postfix(PnlMusicSearchItem __instance)
        {
            AutoCompleteManager.StopCurrentAutoComplete();
        }
    }
}
