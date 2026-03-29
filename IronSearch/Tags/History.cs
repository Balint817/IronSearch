using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHistory(MusicInfo musicInfo)
        {
            return RefreshPatch.history.Contains(musicInfo.uid);
        }
        internal static bool EvalHistory(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "History()");
            ThrowIfNotEmpty(varKwargs, "History()");
            return EvalHistory(M.I);
        }
    }
}
