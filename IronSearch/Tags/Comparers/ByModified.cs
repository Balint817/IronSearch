using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalByModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return (Comparison<MusicInfo>)SorterMethods.SortByModified;
        }
    }
}
