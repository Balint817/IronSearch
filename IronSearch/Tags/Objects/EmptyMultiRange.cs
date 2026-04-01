using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalEmptyMultiRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "EmptyMultiRange", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "EmptyMultiRange", varArgs, varKwargs);
            return MultiRange.EmptyRange;
        }
    }
}
