using Il2CppAssets.Scripts.Database;
using IronSearch.Loaders;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalChartData(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "ChartData", varArgs, varKwargs);
            ThrowIfNotEmpty(varArgs, "ChartData", varArgs, varKwargs);
            if (!ChartDataLoader.VanillaCache!.TryGetValue(M.I.uid, out var data))
            {
                if (!ModMain.CustomAlbumsLoaded || !ChartDataLoader.CustomCache.TryGetValue(M.I.uid, out data))
                {
                    return false;
                }
            }
#pragma warning disable CS8603 // Possible null reference return.
            return data;
#pragma warning restore CS8603
        }
    }
}
