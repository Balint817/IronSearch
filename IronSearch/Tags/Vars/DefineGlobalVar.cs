using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalDefineGlobalVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "DefineGlobal()");
            ThrowIfNotEmpty(varKwargs, "DefineGlobal()");

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "DefineGlobal()");
            }
            GlobalVariables.TryAdd(s, varArgs[1]);

            return true;
        }
    }
}
