using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalStreamer(MusicInfo musicInfo)
        {
            return SearchResults_RefreshPatch.streamer?.Contains(musicInfo.uid) ?? false;
        }
        internal static bool EvalStreamer(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Streamer", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Streamer", varArgs, varKwargs);
            return EvalStreamer(M.I);
        }
    }
}
