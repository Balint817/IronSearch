using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.Structs.Modules;
using Il2CppPeroTools2.PeroString;
using IronSearch.Records;
using IronSearch.Tags;
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
                if (ex is TerminateSearchException safeException)
                {
                    __result = safeException.IsTrue;
                }
                else
                {
                    searchError = new SearchResponse(ex, SearchResponse.Type.RuntimeError);
                }
            }
            return false;
        }
    }
}