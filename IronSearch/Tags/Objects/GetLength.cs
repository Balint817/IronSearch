using Il2CppAssets.Scripts.Database;
using IronSearch.Loaders;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetLength(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "GetLength", varArgs, varKwargs);
            ThrowIfNotEmpty(varArgs, "GetLength", varArgs, varKwargs);

            var l = ChartDataLoader.GetMusicLength(M.I);
            if (l is { } ts)
            {
                return ts.TotalSeconds;
            }
            return null!;
        }
    }
}
