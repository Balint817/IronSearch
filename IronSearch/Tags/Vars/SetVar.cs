using IronSearch.Exceptions;
using System.Collections.Concurrent;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, Dictionary<string, dynamic>> LocalVariables = new();
        internal static dynamic EvalSetVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "SetVar", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "SetVar", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "SetVar", varArgs, varKwargs);
            }

            var d = LocalVariables.GetOrAdd(M.I.uid, _ => new());

            d[s] = varArgs[1];
            return true;
        }
    }
}
