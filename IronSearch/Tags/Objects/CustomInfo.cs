using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalCustomInfo(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "EmptyMultiRange", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "EmptyMultiRange", varArgs, varKwargs);
            if (!ModMain.CustomAlbumsLoaded || !ModMain.uidToCustom.TryGetValue(M.I.uid, out var value))
            {
                return null!;
            }
            return value;
        }
    }
}
