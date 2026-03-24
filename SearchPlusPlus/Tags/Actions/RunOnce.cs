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
            if (!varKwargs.ContainsKey("id"))
            {
                throw new SearchInputException("missing 'id' from RunOnce");
            }
            if (varKwargs["id"] is not string id)
            {
                throw new SearchInputException("invalid RunOnce ID");
            }
            varKwargs.Remove("id");

            ThrowIfNotEmpty(varKwargs);
            ThrowIfNotMatching(varArgs, 1);

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

            if (runOnceIds.TryAdd(id, false))
            {
                varArgs[0]();
            }

            return true;
        }
    }
}
