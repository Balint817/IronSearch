using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using IronPython.Runtime.Operations;
using IronSearch.Patches;
using IronSearch.Records;
using MelonLoader;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> uniqueLogs = new();
        internal static bool EvalLogUnique(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
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
            ThrowIfNotEmpty(varKwargs);

            var sb = new StringBuilder();
            foreach (var item in varArgs)
            {
                sb.Append(item);
                sb.Append(separator);
            }
            var result = sb.ToString();
            if (uniqueLogs.TryAdd(result, false))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, result);
            }
            return true;
        }
    }
}
