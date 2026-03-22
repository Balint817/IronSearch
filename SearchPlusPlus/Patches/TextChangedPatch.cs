using System.Collections.Generic;
using System;
using System.Linq;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.Structs.Modules;
using Il2CppPeroPeroGames;
using CustomAlbums;
using MelonLoader;
using Newtonsoft.Json;
using System.IO;
using Il2CppAssets.Scripts.PeroTools.Nice.Interface;
using Il2CppMono;
using Il2CppSystem.Security.Util;
using Newtonsoft.Json.Linq;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppPeroTools2.PeroString;
using IronSearch.Records;
using PythonExpressionManager;
using Il2CppAssets.Scripts.UI.Panels.PnlMusicTag;

namespace IronSearch.Patches
{

    [HarmonyLib.HarmonyPatch(typeof(PnlMusicSearchItem), "OnTextChanged")]
    internal class TextChangedPatch
    {
        static float? defaultValue = null;
        static long? defaultLValue = null;
        internal static void Prefix(PnlMusicSearchItem __instance, string text)
        {
            if (!text.StartsWith(ModMain.StartString))
            {
                if (defaultValue is { } reset)
                {
                    __instance.m_CoolDownTime = reset;
                    __instance.m_LCoolDownTime = defaultLValue!.Value;
                }
                return;
            }
            if (defaultValue is not { } value)
            {
                defaultValue = __instance.m_CoolDownTime;
                defaultLValue = __instance.m_LCoolDownTime;
            }

            __instance.m_CoolDownTime = defaultValue.Value*ModMain.WaitMultiplierFloat;
            __instance.m_LCoolDownTime = (long)(defaultLValue!.Value*ModMain.WaitMultiplierFloat);

        }
    }
}