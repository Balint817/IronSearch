using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetBPM(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "GetBPM", varArgs, varKwargs);
            if (varArgs[0] is MusicInfo mi)
            {
                return EvalGetBPM(new(M.I, null!), Array.Empty<dynamic>(), varKwargs);
            }
            ThrowIfNotEmpty(varArgs, "GetBPM", varArgs, varKwargs);
            AddBPMInfo(M.I);
            return bpmDict[M.I.uid]!;
        }
    }
}
