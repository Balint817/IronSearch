using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.Structs.Modules;
using Il2CppPeroTools2.PeroString;
using IronSearch.Exceptions;
using IronSearch.Records;
using PythonExpressionManager;

namespace IronSearch.Patches
{

    [HarmonyLib.HarmonyPatch(typeof(SearchResults), "PeroLevelDesigner")]
    internal static class SearchPatch
    {

        internal static CompiledScript? compiledScript { get; set; }

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
                    if (compiledScript == null)
                    {
                        return __result = false;
                    }
                    break;
            }

            __result = false;

            if (compiledScript is null)
            {
                __result = false;
                return false;
            }

            try
            {
                var task = Task.Run(() =>
                    ModMain.ScriptManager.ScriptExecutor.Evaluate(
                        new SearchArgument(musicInfo, peroString), compiledScript)
                );
                if (task.Wait(TimeSpan.FromSeconds(10)))
                {
                    __result = task.GetAwaiter().GetResult();
                }
                else
                {
                    searchError = new SearchResponse("The search timed out.", SearchResponse.Type.TimeoutError);
                    return false;
                }
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