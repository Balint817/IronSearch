using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalDefineVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 2, "DefineVar", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "DefineVar", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "DefineVar", varArgs, varKwargs);
            }
            LocalVariables.TryAdd(M.I.uid, new());

            var d = LocalVariables[M.I.uid];

            if (d.TryGetValue(s, out var v))
            {
                return true;
            }

            d[M.I.uid][s] = varArgs[1];

            return true;
        }
    }
}
