using System.Collections.Concurrent;
using System.Text;
using IronPython.Runtime;
using IronSearch.Records;
using MelonLoader;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> runOnceIds = new();
        internal static bool EvalRunOnce(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 2);

            if (!Utils.IsCallable(varArgs[0]))
            {
                throw new SearchInputException("invalid RunOnce function");
            }
            if (varArgs[0] is Delegate d)
            {
                if (d.Method.ContainsGenericParameters || d.Method.IsAbstract)
                {
                    throw new SearchInputException("invalid RunOnce function");
                }
            }
            else
            {
                if (Utils.GetPythonArgCount(varArgs[0]) != 0)
                {
                    throw new SearchInputException("invalid RunOnce function");
                }
            }
            if (varArgs[1] is not string id)
            {
                throw new SearchInputException("invalid RunOnce id");
            }

            if (runOnceIds.TryAdd(id, false))
            {
                varArgs[0]();
            }

            return true;
        }
    }
}
