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