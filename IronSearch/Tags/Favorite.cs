using Il2CppAssets.Scripts.Database;
using IronSearch.Core;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalFavorite(MusicInfo musicInfo)
        {
            return ActiveSearch.favorites.Contains(musicInfo.uid);
        }
        internal static bool EvalFavorite(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Favorite", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Favorite", varArgs, varKwargs);
            return EvalFavorite(M.I);
        }
    }
}
