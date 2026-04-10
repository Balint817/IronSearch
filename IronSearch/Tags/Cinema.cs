using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static HashSet<string> hasCinema { get; set; } = new();

        internal static bool EvalCinema(MusicInfo musicInfo)
        {
            if (!EvalCustom(musicInfo))
            {
                return false;
            }
            return EvalCinemaInternal(musicInfo);
        }
        internal static bool EvalCinemaInternal(MusicInfo musicInfo)
        {
            return hasCinema.Contains(musicInfo.uid);
        }
        internal static bool EvalCinema(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Cinema", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Cinema", varArgs, varKwargs);
            return EvalCinema(M.I);
        }
    }
}
