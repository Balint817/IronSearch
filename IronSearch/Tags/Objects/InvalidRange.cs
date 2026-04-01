using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalInvalidRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "InvalidRange", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "InvalidRange", varArgs, varKwargs);
            return Range.InvalidRange;
        }
    }
}
