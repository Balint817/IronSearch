using IronSearch.Exceptions;
using System.Collections.Concurrent;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> runOnceIds = new();
        internal static bool EvalRunOnce(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "RunOnce", varArgs, varKwargs);
            ThrowIfNotMatching(varArgs, 2, "RunOnce", varArgs, varKwargs);

            if (!Utils.IsCallable(varArgs[0]))
            {
                throw new SearchValidationException("RunOnce() requires a function with no arguments as the first argument.", "RunOnce", varArgs, varKwargs);
            }
            if (varArgs[0] is Delegate d)
            {
                if (d.Method.ContainsGenericParameters || d.Method.IsAbstract)
                {
                    throw new SearchValidationException("RunOnce() cannot use an abstract or open generic delegate.", "RunOnce", varArgs, varKwargs);
                }
                if (d.Method.GetParameters().Length != 0)
                {
                    throw new SearchValidationException("RunOnce() requires a function with no arguments as the first argument.", "RunOnce", varArgs, varKwargs);
                }
            }
            else
            {
                if (Utils.GetPythonArgCount(varArgs[0]) != 0)
                {
                    throw new SearchValidationException("RunOnce() requires a function with no arguments as the first argument.", "RunOnce", varArgs, varKwargs);
                }
            }
            if (varArgs[1] is not string id)
            {
                throw new SearchWrongTypeException("a string id as the second argument", varArgs[1]?.GetType(), "RunOnce", varArgs, varKwargs);
            }

            if (runOnceIds.TryAdd(id, false))
            {
                varArgs[0]();
            }

            return true;
        }
    }
}
