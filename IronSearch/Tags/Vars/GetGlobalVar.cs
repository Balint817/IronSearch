using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "GetGlobal()");
            ThrowIfNotEmpty(varKwargs, "GetGlobal()");

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "GetGlobal()");
            }
            if (GlobalVariables.TryGetValue(s, out var v))
            {
                return v;
            }
            throw new SearchReferenceException(s, SearchReferenceException.ReferenceKind.Global, "GetGlobal()");
        }
    }
}
