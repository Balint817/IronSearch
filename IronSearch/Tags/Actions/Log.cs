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
                    throw new SearchWrongTypeException("a string for `sep=`", varKwargs["sep"]?.GetType(), "Log", varArgs, varKwargs);
                }
                separator = s;
                varKwargs.Remove("sep");
            }
            ThrowIfNotEmpty(varKwargs, "Log", varArgs, varKwargs);

            MelonLogger.Msg(ConsoleColor.DarkCyan, string.Join(separator, varArgs.Select(x => ((object)x)?.ToString() ?? "None")));
            return true;
        }
    }
}
