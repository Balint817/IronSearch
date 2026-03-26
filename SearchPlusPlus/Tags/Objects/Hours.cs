using IronSearch.Exceptions;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalHours(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);
            if (varArgs[0] is int n)
            {
                return TimeSpan.FromHours(n).Ticks;
            }
            throw new SearchWrongTypeException("an integer number of hours", varArgs[0]?.GetType(), "Hours()");
        }
    }
}
