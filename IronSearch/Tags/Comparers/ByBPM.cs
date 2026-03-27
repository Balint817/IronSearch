using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalByBPM(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return (Comparison<MusicInfo>)SearchMergePatch.SortByBPM;
        }
    }
}
