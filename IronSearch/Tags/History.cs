using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHistory(MusicInfo musicInfo)
        {
            return SearchResults_RefreshPatch.history.Contains(musicInfo.uid);
        }
        internal static bool EvalHistory(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "History", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "History", varArgs, varKwargs);
            return EvalHistory(M.I);
        }
    }
}
