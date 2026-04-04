using Il2CppAssets.Scripts.Database;
using IronSearch.Core;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalStreamer(MusicInfo musicInfo)
        {
            return ActiveSearch.streamer?.Contains(musicInfo.uid) ?? false;
        }
        internal static bool EvalStreamer(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Streamer", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Streamer", varArgs, varKwargs);
            return EvalStreamer(M.I);
        }
    }
}
