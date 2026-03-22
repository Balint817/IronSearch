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

namespace IronSearch.Patches
{

    [HarmonyLib.HarmonyPatch(typeof(SearchResults), "PeroLevelDesigner")]
    internal static class SearchPatch
    {

        internal static CompiledScript? tagGroups { get; set; }

        internal static bool? isAdvancedSearch = false;

        internal static SearchResponse? searchError = null;
        internal static bool Prefix(ref bool __result, PeroString peroString, MusicInfo musicInfo, string containsText)
        {
            if (searchError != null)
            {
                return __result = false;
            }
            switch (isAdvancedSearch)
            {
                case null:
                    __result = true;
                    return false;
                case false:
                    return false;
                case true:
                    if (tagGroups == null)
                    {
                        return __result = false;
                    }
                    break;
            }

            __result = false;

            if (tagGroups is null)
            {
                __result = false;
                return false;
            }

            try
            {
                var searchResult = ModMain.ScriptManager.ScriptExecutor.Evaluate(new SearchArgument(musicInfo, peroString), tagGroups);
                __result = searchResult;
            }
            catch (Exception ex)
            {
                searchError = new SearchResponse(ex, SearchResponse.Type.RuntimeError);
            }
            return false;
        }
    }
}