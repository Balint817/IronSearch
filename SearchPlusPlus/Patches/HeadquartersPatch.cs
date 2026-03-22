using System.Text.Json;
using System.Text.RegularExpressions;
using CustomAlbums;
using HarmonyLib;
using IronSearch.Records.HQMD5;
using MelonLoader;

namespace IronSearch.Patches
{
    internal class HeadquartersPatch
    {
        static readonly Regex targetURLRegex = new('^' + Regex.Escape(Headquarters.Main.ApiPrefix + "/sheets/") + "[a-fA-F0-9]+$");
        static void Prefix(string url, Action<JsonDocument, bool> callback)
        {
            if (targetURLRegex.IsMatch(url))
            {
                var orig = callback;
                callback = (a,b) =>
                {
                    try
                    {
                        var response = a.Deserialize<MD5Response>()!;
                        if (response.chart.ranked is { } isRanked)
                        {
                            ModMain._hqChartDict[response.hash] = isRanked;
                        }
                    }
                    catch (Exception)
                    {
                        //catch silently
                    }
                    orig(a,b);
                };
            }
        }
        internal static void RunPatch(HarmonyLib.Harmony harmonyInstance)
        {
            try
            {
                var hqType = AccessTools.TypeByName("Headquarters.Utilities.Web");
                var getMethod = hqType.GetMethod("Get");
                var prefix = Prefix;
                harmonyInstance.Patch(getMethod, new HarmonyMethod(prefix.Method));
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(ConsoleColor.Red, ex);
                MelonLogger.Msg(ConsoleColor.Red, "Error occured while patching HQ, ranking information will not be able to refresh.");
            }

        }
    }
}