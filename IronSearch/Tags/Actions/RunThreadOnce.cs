using IronSearch.Exceptions;
using IronSearch.Utils;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static ThreadLocal<Dictionary<string, bool>> runThreadOnceTracker = null!;
        internal static readonly Range _runThreadOnceArgsRange = new(1, 2);
        internal static bool EvalRunThreadOnce(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "RunThreadOnce", varArgs, varKwargs);
            ThrowIfNotMatching(varArgs, _runThreadOnceArgsRange, "RunThreadOnce", varArgs, varKwargs);

            if (!PythonUtils.IsCallable(varArgs[0]))
            {
                throw new SearchValidationException("RunThreadOnce() requires a function with no arguments as the first argument.", "RunThreadOnce", varArgs, varKwargs);
            }
            if (varArgs[0] is Delegate d)
            {
                if (d.Method.ContainsGenericParameters || d.Method.IsAbstract)
                {
                    throw new SearchValidationException("RunThreadOnce() cannot use an abstract or open generic delegate.", "RunThreadOnce", varArgs, varKwargs);
                }
                if (d.Method.GetParameters().Length != 0)
                {
                    throw new SearchValidationException("RunThreadOnce() requires a function with no arguments as the first argument.", "RunThreadOnce", varArgs, varKwargs);
                }
            }
            else
            {
                if (PythonUtils.GetPythonArgCount(varArgs[0]) != 0)
                {
                    throw new SearchValidationException("RunThreadOnce() requires a function with no arguments as the first argument.", "RunThreadOnce", varArgs, varKwargs);
                }
            }
            string id = "";
            if (varArgs.Length == 2 && varArgs[1] is string s)
            {
                id = s;
            }

            if (runThreadOnceTracker.Value!.TryAdd(id, false))
            {
                varArgs[0]();
            }

            return true;
        }
    }
}
