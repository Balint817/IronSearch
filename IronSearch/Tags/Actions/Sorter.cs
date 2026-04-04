using IronPython.Runtime.Operations;
using IronSearch.Core;
using IronSearch.Exceptions;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalSorter(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfEmpty(varArgs, "Sorter", varArgs, varKwargs);

            bool reverse = false;
            int priority = 0;
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
                    throw new SearchWrongTypeException("an integer for `priority=`", t?.GetType(), "Sorter", varArgs, varKwargs);
                }
                priority = tn;
                varKwargs.Remove("priority");
            }
            ThrowIfNotEmpty(varKwargs, "Sorter", varArgs, varKwargs);

            var args = varArgs.ToList();
            if (args[^1] is bool b1)
            {
                reverse = b1;
                args.RemoveAt(args.Count - 1);
                if (args.Count != 0 && args[^1] is int n1)
                {
                    priority = n1;
                    args.RemoveAt(args.Count - 1);
                }
            }
            else if (args[^1] is int n1)
            {
                priority = n1;
                args.RemoveAt(args.Count - 1);
                if (args.Count != 0 && args[^1] is bool b2)
                {
                    reverse = b2;
                    args.RemoveAt(args.Count - 1);
                }
            }

            ThrowIfEmpty(varArgs, "Sorter", varArgs, varKwargs);

            lock (ActiveSearch._activeSorters)
            {
                ActiveSearch._activeSorters.Add(new(varArgs, reverse, priority));
            }

            return true;
        }
    }
}
