using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalFullMultiRange(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "FullMultiRange", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "FullMultiRange", varArgs, varKwargs);
            return MultiRange.FullRange;
        }
    }
}
