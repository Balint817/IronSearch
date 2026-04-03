using Il2CppAssets.Scripts.Database;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalByAccuracy(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return (Comparison<MusicInfo>)SorterMethods.SortByAccuracy;
        }
    }
}
