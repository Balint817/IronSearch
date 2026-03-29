using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppPeroPeroGames.GlobalDefines;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetBPM(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetBPM()");
            ThrowIfNotEmpty(varKwargs, "GetBPM()");
            AddBPMInfo(M.I);
            return bpmDict[M.I.uid]!;
        }
    }
}
