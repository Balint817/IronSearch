using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalWeeks(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);
            if (varArgs[0] is int n)
            {
                return TimeSpan.FromDays(n).Ticks * 7;
            }
            throw new SearchInputException("expected integer as time unit multiple");
        }
    }
}
