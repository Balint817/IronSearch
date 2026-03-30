using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppPeroPeroGames.GlobalDefines;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetLanguageIndex(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetLanguage()");
            ThrowIfNotEmpty(varKwargs, "GetLanguage()");
            return RefreshPatch.langIndex;
        }
    }
}
