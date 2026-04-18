using Il2CppAssets.Scripts.Database;
using IronSearch.Loaders;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalDialogue(MusicInfo musicInfo)
        {
            if (!ChartDataLoader.VanillaCache!.TryGetValue(musicInfo.uid, out var data))
            {
                if (!ModMain.CustomAlbumsLoaded || !ChartDataLoader.CustomCache.TryGetValue(musicInfo.uid, out data))
                {
                    return false;
                }
            }
            return data?.HasDialogue ?? false;
        }
        internal static bool EvalDialogue(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Dialogue", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Dialogue", varArgs, varKwargs);
            return EvalDialogue(M.I);
        }
    }
}
