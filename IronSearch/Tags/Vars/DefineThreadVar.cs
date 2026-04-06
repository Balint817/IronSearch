using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalDefineThreadVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "DefineThreaded", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "DefineThreaded", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "DefineThreaded", varArgs, varKwargs);
            }

            ThreadLocalVariables.Value!.TryAdd(s, varArgs[1]);

            return true;
        }
    }
}
