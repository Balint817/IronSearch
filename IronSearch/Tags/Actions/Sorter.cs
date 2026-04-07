using IronPython.Runtime.Operations;
using IronSearch.Core;
using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalSorter(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfEmpty(varArgs, "Sort", varArgs, varKwargs);

            bool reverse = false;
            int priority = 0;
            string id = "";
            if (varKwargs.ContainsKey("reverse"))
            {
                reverse = PythonOps.IsTrue(varKwargs["reverse"]);
                varKwargs.Remove("reverse");
            }
            if (varKwargs.ContainsKey("priority"))
            {
                var t = varKwargs["priority"];
                if (t is not int tn)
                {
                    throw new SearchWrongTypeException("an integer for `priority=`", t?.GetType(), "Sort", varArgs, varKwargs);
                }
                priority = tn;
                varKwargs.Remove("priority");
            }
            if (varKwargs.ContainsKey("id"))
            {
                var t = varKwargs["id"];
                if (t is not string s)
                {
                    throw new SearchWrongTypeException("a string for `id=`", t?.GetType(), "Sort", varArgs, varKwargs);
                }
                id = s;
                varKwargs.Remove("id");
            }
            ThrowIfNotEmpty(varKwargs, "Sort", varArgs, varKwargs);

            ActiveSearch._activeSorters.TryAdd(id, new());
            var dict = ActiveSearch._activeSorters[id];
            if (!dict.ContainsKey(priority))
            {
                dict.TryAdd(priority, new(varArgs, reverse, priority));
            }

            return true;
        }
    }
}
