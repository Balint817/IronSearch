using IronSearch.Exceptions;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        private static readonly object runSyncLock = new();
        internal static bool EvalRunSync(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            lock (runSyncLock)
            {
                ThrowIfNotEmpty(varKwargs, "RunSync", varArgs, varKwargs);
                ThrowIfNotMatching(varArgs, 1, "RunSync", varArgs, varKwargs);

                if (!PythonUtils.IsCallable(varArgs[0]))
                {
                    throw new SearchValidationException("RunSync() requires a function with no arguments as the argument.", "RunSync", varArgs, varKwargs);
                }
                if (varArgs[0] is Delegate d)
                {
                    if (d.Method.ContainsGenericParameters || d.Method.IsAbstract)
                    {
                        throw new SearchValidationException("RunSync() cannot use an abstract or open generic delegate.", "RunSync", varArgs, varKwargs);
                    }
                    if (d.Method.GetParameters().Length != 0)
                    {
                        throw new SearchValidationException("RunSync() requires a function with no arguments as the first argument.", "RunSync", varArgs, varKwargs);
                    }
                }
                else
                {
                    if (PythonUtils.GetPythonArgCount(varArgs[0]) != 0)
                    {
                        throw new SearchValidationException("RunSync() requires a function with no arguments as the first argument.", "RunSync", varArgs, varKwargs);
                    }
                }

                varArgs[0]();
            }

            return true;
        }
    }
}
