using System.Text;
using IronSearch.Exceptions;
using MelonLoader;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static bool EvalLog(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            var separator = " ";
            if (varKwargs.ContainsKey("sep"))
            {
                if (varKwargs["sep"] is not string s)
                {
                    throw new SearchWrongTypeException("a string for `sep=`", varKwargs["sep"]?.GetType(), "Log()");
                }
                separator = s;
                varKwargs.Remove("sep");
            }
            ThrowIfNotEmpty(varKwargs, "Log()");

            var sb = new StringBuilder();
            foreach (var item in varArgs)
            {
                sb.Append((object)item);
                sb.Append(separator);
            }
            MelonLogger.Msg(ConsoleColor.DarkCyan, sb.ToString());
            return true;
        }
    }
}
