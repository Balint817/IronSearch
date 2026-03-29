using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalFavorite(MusicInfo musicInfo)
        {
            return RefreshPatch.favorites.Contains(musicInfo.uid);
        }
        internal static bool EvalFavorite(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Favorite()");
            ThrowIfNotEmpty(varKwargs, "Favorite()");
            return EvalFavorite(M.I);
        }
    }
}
