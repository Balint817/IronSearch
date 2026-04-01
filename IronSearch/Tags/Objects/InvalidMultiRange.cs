using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalInvalidMultiRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "InvalidMultiRange", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "InvalidMultiRange", varArgs, varKwargs);
            return MultiRange.InvalidRange;
        }
    }
}
