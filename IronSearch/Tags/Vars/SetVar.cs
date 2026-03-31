using System.Collections.Concurrent;
using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, Dictionary<string, dynamic>> LocalVariables = new();
        internal static dynamic EvalSetVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "SetVar()");
            ThrowIfNotEmpty(varKwargs, "SetVar()");

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "SetVar()");
            }

            LocalVariables.TryAdd(M.I.uid, new());

            LocalVariables[M.I.uid][s] = varArgs[1];
            return true;
        }
    }
}
