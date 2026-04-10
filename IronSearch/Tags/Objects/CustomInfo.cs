namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalCustomInfo(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "CustomInfo", varArgs, varKwargs);
            ThrowIfNotEmpty(varArgs, "CustomInfo", varArgs, varKwargs);
            if (!ModMain.CustomAlbumsLoaded || !ModMain.uidToCustom.TryGetValue(M.I.uid, out var value))
            {
                return null!;
            }
            return value;
        }
    }
}
