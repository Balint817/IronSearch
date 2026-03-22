using System.Collections.Concurrent;
using System.Text;
using IronSearch.Records;
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
                    throw new SearchInputException("invalid separator type");
                }
                separator = s;
                varKwargs.Remove("sep");
            }
            if (!varKwargs.ContainsKey("id"))
            {
                throw new SearchInputException("missing 'id' from LogOnce");
            }
            if (varKwargs["id"] is not string id)
            {
                throw new SearchInputException("invalid LogOnce ID");
            }
            varKwargs.Remove("id");

            ThrowIfNotEmpty(varKwargs);

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
