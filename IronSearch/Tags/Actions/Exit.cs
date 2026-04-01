using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalExit(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "Exit", varArgs, varKwargs);
            if (varArgs[0] is not bool b)
            {
                throw new SearchWrongTypeException("True or False for whether to exit the search", varArgs[0]?.GetType(), "Exit", varArgs, varKwargs);
            }
            throw new TerminateSearchException(b);
        }
    }
}
