using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalInvalidMultiRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "InvalidMultiRange()");
            ThrowIfNotEmpty(varKwargs, "InvalidMultiRange()");
            return MultiRange.InvalidRange;
        }
    }
}
