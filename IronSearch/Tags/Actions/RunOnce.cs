using System.Collections.Concurrent;
using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> runOnceIds = new();
        internal static bool EvalRunOnce(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "RunOnce()");
            ThrowIfNotMatching(varArgs, 2, "RunOnce()");

            if (!Utils.IsCallable(varArgs[0]))
            {
                throw new SearchValidationException("RunOnce() requires a function with no arguments as the first argument.", "RunOnce()");
            }
            if (varArgs[0] is Delegate d)
            {
                if (d.Method.ContainsGenericParameters || d.Method.IsAbstract)
                {
                    throw new SearchValidationException("RunOnce() cannot use an abstract or open generic delegate.", "RunOnce()");
                }
                if (d.Method.GetParameters().Length != 0)
                {
                    throw new SearchValidationException("RunOnce() requires a function with no arguments as the first argument.", "RunOnce()");
                }
            }
            else
            {
                if (Utils.GetPythonArgCount(varArgs[0]) != 0)
                {
                    throw new SearchValidationException("RunOnce() requires a function with no arguments as the first argument.", "RunOnce()");
                }
            }
            if (varArgs[1] is not string id)
            {
                throw new SearchWrongTypeException("a string id as the second argument", varArgs[1]?.GetType(), "RunOnce()");
            }

            if (runOnceIds.TryAdd(id, false))
            {
                varArgs[0]();
            }

            return true;
        }
    }
}
