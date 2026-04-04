using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static dynamic EvalGetVar(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "GetVar", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "GetVar", varArgs, varKwargs);

            if (varArgs[0] is not string s)
            {
                throw new SearchWrongTypeException("a string variable name", varArgs[0]?.GetType(), "GetVar", varArgs, varKwargs);
            }

            var d = LocalVariables.GetOrAdd(M.I.uid, _=>new());

            if (d.TryGetValue(s, out var v))
            {
                return v;
            }
            throw new SearchReferenceException(s, SearchReferenceException.ReferenceKind.Local, "GetVar", varArgs, varKwargs);
        }
    }
}
