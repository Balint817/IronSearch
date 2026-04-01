namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalHasTouhou(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Touhou", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "Touhou", varArgs, varKwargs);

            Utils.GetAvailableMaps(M.I, out var availableMaps);
            return availableMaps.Contains(4);
        }
    }
}
