using IronSearch.Exceptions;
using System.Collections.Concurrent;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly Dictionary<string, dynamic> GlobalVariables = new();
        private static readonly object _globalVarLock = new();
        internal static dynamic EvalSetGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            lock (_globalVarLock)
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
}
