using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalFullRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "FullRange()");
            ThrowIfNotEmpty(varKwargs, "FullRange()");
            return Range.FullRange;
        }
    }
}
