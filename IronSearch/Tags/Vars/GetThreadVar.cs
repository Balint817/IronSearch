using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetThreadVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "GetThreaded", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "GetThreaded", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "GetThreaded", varArgs, varKwargs);
            }

            if (ThreadLocalVariables.Value!.TryGetValue(s, out var v))
            {
                return v;
            }

            throw new SearchReferenceException(s, SearchReferenceException.ReferenceKind.ThreadGlobal, "GetThreaded", varArgs, varKwargs);
        }
    }
}
