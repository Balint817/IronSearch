namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static bool EvalHasHidden(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Hidden", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Hidden", varArgs, varKwargs);

            Utils.GetAvailableMaps(M.I, out var availableMaps);
            return availableMaps.Contains(4);
        }
    }
}
