using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronSearch.Records;
using MelonLoader;
using Newtonsoft.Json;
using PythonExpressionManager;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text;
using Range = IronSearch.Records.Range;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        internal static readonly ConcurrentDictionary<string, bool> helpIds = new();
        internal static bool helpEnabled = true;
        internal static Mutex helpMutex = new();
        internal static bool EvalHelp(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotMatching(varArgs, 1, "Help", varArgs, varKwargs);

            try
            {
                helpMutex.WaitOne();
                EvalHelpInternal(varArgs[0]);
            }
            finally
            {
                helpMutex.ReleaseMutex();
            }


            return true;
        }

        internal static void EvalHelpInternal(dynamic arg0)
        {

            if (!helpEnabled)
            {
                return;
            }

            var matched = true;
            var incorrectUsageHelp = "If this was not what you wanted to know, you might have done Help(Name) or Help(Name()), for example, but the correct usage is Help(\"Name\")";
            switch (arg0)
            {
                case null:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object 'None' is, well, None (or null)!");
                    break;
                case int varInt:
                case long varLong:
                case BigInteger varBigInteger:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object {arg0} is an integer!");
                    break;
                case float varFloat:
                case double varDouble:
                case decimal varDecimal:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object {arg0} is a real (or floating point) number!");
                    break;
                case bool varBool:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object {arg0} is an True/False (boolean) value!");
                    break;
                case byte[] varBytes:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object {JsonConvert.SerializeObject(varBytes)} is a bytes-like object (or bytearray, list of bytes, etc.)!");
                    break;
                case PythonList varPythonList:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object {PythonOps.ToString(arg0)} is a list!");
                    break;
                case PythonDictionary varPythonDictionary:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object {PythonOps.ToString(arg0)} is a dictionary!");
                    break;
                case Range varRange:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object Range('{arg0}') is a Range!");
                    break;
                case MultiRange varMultiRange:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"The object Range('{arg0}') is a MultiRange!");
                    break;
                case SearchArgument varSearchArgument:
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"This is the current song's details!");
                    break;
                case string varString:
                    matched = false;
                    break;
                default:
                    if (Utils.IsCallable(arg0))
                    {
                        MelonLogger.Msg(ConsoleColor.DarkCyan, $"This is a function!");
                    }
                    else
                    {
                        MelonLogger.Msg(ConsoleColor.DarkCyan, $"I have no clue what a \"{arg0.GetType()}\" is, but it's probably not what you wanted to send here!");
                    }
                    break;
            }

            if (matched)
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, incorrectUsageHelp);
                helpEnabled = false;
                return;
            }

            var functionName = ((string)arg0!).Trim();
            if (functionName.Contains('('))
            {
                functionName = functionName.Split('(')[0];
            }

            if (helpIds.TryAdd(functionName, false))
            {
                if (!functionName.IsValidVariableName())
                {
                    MelonLogger.Msg(ConsoleColor.DarkCyan, $"This isn't a valid variable name: " + functionName);
                    return;
                }
                string? unaliasedName = null;
                if (!ModMain.HelpStrings.TryGetValue(functionName, out var helpString))
                {
                    if (!ModMain.Aliases.TryGetValue(functionName, out unaliasedName) || !ModMain.HelpStrings.TryGetValue(unaliasedName, out helpString))
                    {
                        MelonLogger.Msg(ConsoleColor.DarkCyan, $"It seems \"{functionName}\" does not work here (or if they do, they never told anybody what they actually do around here)");
                        return;
                    }

                }
                var sb = new StringBuilder();
                sb.Append($"Help(\"{functionName}\"):");
                if (unaliasedName is not null)
                {
                    sb.Append($" (alias for \"{unaliasedName}\")");
                }
                sb.Append($"\n{helpString}");
                MelonLogger.Msg(ConsoleColor.DarkCyan, sb.ToString());
            }
        }
    }
}
