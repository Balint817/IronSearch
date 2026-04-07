using IronSearch.Exceptions;
using MelonLoader;
using System.Collections.Concurrent;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> logUnique = new();
        internal static bool EvalLogUnique(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            var separator = " ";
            if (varKwargs.ContainsKey("sep"))
            {
                if (varKwargs["sep"] is not string s)
                {
                    throw new SearchWrongTypeException("a string for `sep=`", varKwargs["sep"]?.GetType(), "LogUnique", varArgs, varKwargs);
                }
                separator = s;
                varKwargs.Remove("sep");
            }
            ThrowIfNotEmpty(varKwargs, "LogUnique", varArgs, varKwargs);

            var result = string.Join(separator, varArgs.Select(x => ((object)x)?.ToString() ?? "None"));
            if (logUnique.TryAdd(result, false))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, result);
            }
            return true;
        }
    }
}
