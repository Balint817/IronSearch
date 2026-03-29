using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHasHidden(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Hidden()");
            ThrowIfNotEmpty(varKwargs, "Hidden()");

            Utils.GetAvailableMaps(M.I, out var availableMaps);
            return availableMaps.Contains(4);
        }
    }
}
