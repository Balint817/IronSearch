using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            lock (_globalVarLock)
            {
                ThrowIfNotMatching(varArgs, 1, "GetGlobal", varArgs, varKwargs);
                ThrowIfNotEmpty(varKwargs, "GetGlobal", varArgs, varKwargs);

                if (varArgs[0] is not string s)
                {
                    throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "GetGlobal", varArgs, varKwargs);
                }
                if (GlobalVariables.TryGetValue(s, out var v))
                {
                    return v;
                }
                throw new SearchReferenceException(s, SearchReferenceException.ReferenceKind.Global, "GetGlobal", varArgs, varKwargs);
            }
        }
    }
}
