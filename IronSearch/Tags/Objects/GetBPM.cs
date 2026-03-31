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
