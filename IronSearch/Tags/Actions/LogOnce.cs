using System.Collections.Concurrent;
using System.Text;
using IronSearch.Exceptions;
using MelonLoader;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> logOnceIds = new();
        internal static bool EvalLogOnce(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            var separator = " ";
            if (varKwargs.ContainsKey("sep"))
            {
                if (varKwargs["sep"] is not string s)
                {
                    throw new SearchWrongTypeException("a string for `sep=`", varKwargs["sep"]?.GetType(), "LogOnce", varArgs, varKwargs);
                }
                separator = s;
                varKwargs.Remove("sep");
            }
            if (!varKwargs.ContainsKey("id"))
            {
                throw new SearchValidationException("LogOnce() requires an `id=` keyword so the message is only printed once per id.", "LogOnce", varArgs, varKwargs);
            }
            if (varKwargs["id"] is not string id)
            {
                throw new SearchWrongTypeException("a string for `id=`", varKwargs["id"]?.GetType(), "LogOnce", varArgs, varKwargs);
            }
            varKwargs.Remove("id");

            ThrowIfNotEmpty(varKwargs, "LogOnce", varArgs, varKwargs);

            var sb = new StringBuilder();
            sb.Append(id);
            sb.Append(separator);
            foreach (var item in varArgs)
            {
                sb.Append((object)item);
                sb.Append(separator);
            }

            if (logOnceIds.TryAdd(id, false))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, sb.ToString());
            }

            return true;
        }
    }
}
