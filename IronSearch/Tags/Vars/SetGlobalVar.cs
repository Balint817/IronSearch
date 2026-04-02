using IronSearch.Exceptions;
using System.Collections.Concurrent;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, dynamic> GlobalVariables = new();
        internal static dynamic EvalSetGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "SetGlobal", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "SetGlobal", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "SetGlobal", varArgs, varKwargs);
            }

            GlobalVariables[s] = varArgs[1];
            return true;
        }
    }
}
