namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalHasTouhou(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "Touhou()");
            ThrowIfNotEmpty(varKwargs, "Touhou()");

            Utils.GetAvailableMaps(M.I, out var availableMaps);
            return availableMaps.Contains(4);
        }
    }
}
