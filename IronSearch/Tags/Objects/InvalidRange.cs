using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalInvalidRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "InvalidRange()");
            ThrowIfNotEmpty(varKwargs, "InvalidRange()");
            return Range.InvalidRange;
        }
    }
}
