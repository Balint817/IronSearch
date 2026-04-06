using IronSearch.Exceptions;
using System.Collections.Concurrent;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static ThreadLocal<Dictionary<string, dynamic>> ThreadLocalVariables = null!;
        internal static dynamic EvalSetThreadVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "SetThreaded", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "SetThreaded", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "SetThreaded", varArgs, varKwargs);
            }

            ThreadLocalVariables.Value![s] = varArgs[1];


            return true;
        }
    }
}
