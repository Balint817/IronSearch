using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalDefineGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            lock (_globalVarLock)
            {
                ThrowIfNotMatching(varArgs, 2, "DefineGlobal", varArgs, varKwargs);
                ThrowIfNotEmpty(varKwargs, "DefineGlobal", varArgs, varKwargs);

                if (varArgs[0] is not string s)
                {
                    throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "DefineGlobal", varArgs, varKwargs);
                }
                GlobalVariables.TryAdd(s, varArgs[1]);

                return true;
            }
        }
    }
}
