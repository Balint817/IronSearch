using Il2CppAssets.Scripts.Database;
using Il2CppPeroTools2.PeroString;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronSearch.Records;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Range= IronSearch.Records.Range;

namespace IronSearch.Exceptions
{
    /// <summary>
    /// Base type for user-facing search/tag input errors. Prefer derived types when they fit;
    /// use this when a simple message is enough.
    /// </summary>
    public class SearchInputException : Exception
    {
        /// <summary>
        /// Context for what part of the search failed (e.g. tag name, built-in name, parameter name).
        /// </summary>
        public string ParameterContext { get; }

        /// <summary>The positional arguments that were passed when the error occurred.</summary>
        public dynamic[] VarArgs { get; }

        /// <summary>The keyword arguments that were passed when the error occurred.</summary>
        public Dictionary<string, dynamic> VarKwargs { get; }

        public SearchInputException(string message, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs, Exception? innerException = null)
            : base(message, innerException)
        {
            ParameterContext = parameterContext;
            VarArgs = varArgs;
            VarKwargs = varKwargs;
        }
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Error:")
                .Append(ParameterContext)
                .Append('(');

            var varKwargsArray = VarKwargs.ToArray();

            for (int i = 0; i < VarArgs.Length-1; i++)
            {
                var arg = VarArgs[i];
                sb.Append(FormatArg(arg));
                sb.Append(", ");
            }
            if (VarArgs.Length != 0)
            {
                var arg = VarArgs[^1];
                sb.Append(FormatArg(arg));
                if (varKwargsArray.Length != 0)
                {
                    sb.Append(", ");
                }
            }

            for (int i = 0; i < varKwargsArray.Length-1; i++)
            {
                var kwarg = varKwargsArray[i];
                sb.Append(kwarg.Key).Append('=').Append(FormatArg(kwarg.Value));
                sb.Append(", ");
            }
            if (varKwargsArray.Length != 0)
            {
                var kwarg = varKwargsArray[^1];
                sb.Append(kwarg.Key).Append('=').Append(FormatArg(kwarg.Value));
            }

            sb.Append("): ").AppendLine(Message);
            return sb.ToString();
        }
        private static string FormatArg(dynamic? arg)
        {
            if (arg is null) return "None";
            if (arg is string s) return JsonConvert.SerializeObject(s);
            switch (arg)
            {
                case int or double or bool:
                    return JsonConvert.SerializeObject(arg);
                case System.Numerics.Complex complex:
                    if (complex.Real == 0)
                    {
                        if (complex.Imaginary == 0)
                        {
                            return "0j";
                        }
                        return JsonConvert.SerializeObject(complex.Imaginary)+"j";
                    }
                    else if (complex.Imaginary == 0)
                    {
                        return $"({JsonConvert.SerializeObject(complex.Real)}+0j)";
                    }

                    return $"({JsonConvert.SerializeObject(complex.Real)}+{JsonConvert.SerializeObject(complex.Imaginary)}j)";
                case PythonDictionary pd:
                    if (pd.Count == 0)
                    {
                        return "{}";
                    }
                    return "{...}";
                case PythonList pl:
                    if (pl.Count == 0)
                    {
                        return "[]";
                    }
                    return "[...]";
                case PythonTuple pt:
                    if (pt.Count == 0)
                    {
                        return "()";
                    }
                    return "(...)";
                case SetCollection sc:
                    if (sc.Count == 0)
                    {
                        return "set()";
                    }
                    return "set(...)";
                case PythonFunction pf:
                    if (pf.__name__ is null)
                    {
                        return "<function>";
                    }
                    return $"<function {JsonConvert.SerializeObject(pf.__name__)}>";
                case Range or MultiRange:
                    return arg.ToString();
                case Highscore:
                    return "Score(...)";
                case LocalInfo:
                    return "Local(...)";
                case FuzzyContains fc:
                    return $"Fuzzy({(fc.Pattern is null ? "None" : $"{JsonConvert.SerializeObject(fc.Pattern)}")})";
                case SearchArgument:
                    return $"{ModMain.ArgumentName}";
                case MusicInfo:
                    return $"M.I";
                case PeroString:
                    return $"M.PS";
                case Regex re:
                    return $"Regex({JsonConvert.SerializeObject(re.ToString())})";
                default:
                    try
                    {
                        var typeDynamic = arg.GetType();
                        if (typeDynamic is Type type && (type.Namespace?.StartsWith("IronPython.") ?? false))
                        {
                            return PythonOps.ToString(arg);
                        }
                        return $"<{typeDynamic.ToString()}>";
                    }
                    catch (Exception)
                    {
                        return "<unknown object>";
                    }
            }
        }
    }
}
