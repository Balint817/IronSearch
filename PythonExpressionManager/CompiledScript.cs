using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using static Community.CsharpSqlite.Sqlite3;

namespace PythonExpressionManager
{
    public sealed class CompiledScript
    {
        static CompiledScript()
        {
            var assembly = typeof(TraceBack).Assembly;
            pythonAwareExceptionType = assembly.GetTypes().First(x => x.FullName == "IronPython.Runtime.Exceptions.IPythonAwareException");
            baseExceptionGetterMethod = pythonAwareExceptionType.GetMethod("get_PythonException", BindingFlags.Public | BindingFlags.Instance)!;
            extractMethod = typeof(TraceBack).GetMethod("Extract", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new NullReferenceException();
        }
        //TODO: remove ConvertException, replace with helper method the user can call in their catch block to preserve stack traces, and in general make exceptions more useful.
        const string tagDict = "___";
        const string args = "____";
        const string kwargs = "_____";
        const string f1 = "______";
        const string f2 = "_______";
        const string searchExpressionName = "<expression>";
        internal dynamic Function;
        static readonly Type pythonAwareExceptionType;
        static readonly MethodInfo baseExceptionGetterMethod;
        static readonly MethodInfo extractMethod;
        internal CompiledScript(string body, ScriptExecutor instance)
        {
            var Scripts = new ReadOnlyDictionary<string, dynamic>(instance.RegisteredKeys.ToDictionary(x => x.Key, x => x.Value.Function));


            var scriptBuilder = new StringBuilder(
                $"def {f1}({instance.ArgumentName}, **{instance.BaseDictName}):\n"
                );

            scriptBuilder.AppendLine($"\t{tagDict} = {{}}");
            scriptBuilder.AppendLine($"\ttrue = True");
            scriptBuilder.AppendLine($"\tfalse = False");

            foreach (var item in Scripts)
            {
                scriptBuilder.AppendLine($"\t{tagDict}['{item.Key}'] = {item.Key} = lambda *{args}, **{kwargs}: {instance.BaseDictName}['{item.Key}']({instance.ArgumentName}, {instance.BaseDictName}, *{args}, **{kwargs})");
            }

            scriptBuilder.AppendLine($"\treturn ({body.Replace("\n", "\\n")})");

            scriptBuilder.AppendLine($"\treturn");

            scriptBuilder.AppendLine($"{f2} = lambda {instance.BaseDictName}: (lambda {instance.ArgumentName}: {f1}({instance.ArgumentName}, **{instance.BaseDictName}))");

            var script = scriptBuilder.ToString();
            var source = instance.Engine.CreateScriptSourceFromString(script, searchExpressionName, Microsoft.Scripting.SourceCodeKind.File);
            var scope = instance.Engine.CreateScope();

            source.Execute(scope);
            var functionWrapper = scope.GetVariable(f2);
            Function = functionWrapper(Scripts);
        }

        public static bool TryConvertException(Exception wrappedEx, ScriptEngine? engine = null)
        {
            switch (wrappedEx)
            {
                case SyntaxErrorException syntaxEx:
                    return HandleSyntaxError(syntaxEx);

                case UnboundNameException nameEx:
                    throw new PythonException($"NameError: {nameEx.Message}");

                //case MissingMemberException memberEx:
                //    throw new PythonException($"AttributeError: {memberEx.Message}", memberEx);

                //case KeyNotFoundException keyEx:
                //    throw new PythonException($"KeyError: {keyEx.Message}", keyEx);

                //case IndexOutOfRangeException indexEx:
                //case ArgumentOutOfRangeException argOutEx:
                //    throw new PythonException($"IndexError: {wrappedEx.Message}", wrappedEx);

                //case DivideByZeroException divEx:
                //    throw new PythonException($"ZeroDivisionError: {divEx.Message}", divEx);

                //case InvalidCastException castEx:
                //    throw new PythonException($"TypeError: {castEx.Message}", castEx);

                //case FormatException formatEx:
                //    throw new PythonException($"ValueError: {formatEx.Message}", formatEx);

                //case ArgumentException argEx:
                //    throw new PythonException($"ValueError/TypeError: {argEx.Message}", argEx);

                //case ImportException importEx:
                //    throw new PythonException($"ImportError: {importEx.Message}", importEx);

                //case NullReferenceException nullEx:
                //    throw new PythonException($"AttributeError: {nullEx.Message} (often caused by a None/null value)", nullEx);

                default:
                    if (engine != null)
                    {
                        var exceptionOperations = engine.GetService<ExceptionOperations>();
                        var formatted = exceptionOperations.FormatException(wrappedEx);
                        if (!string.IsNullOrEmpty(formatted))
                        {



                            var formatSplit = formatted.Split(':', 2);
                            if (formatSplit.Length == 2 && !formatSplit[0].Contains("Exception", StringComparison.Ordinal))
                            {
                                formatSplit = formatted.Split('\n');
                                var sb = new StringBuilder();
                                int i = 0;
                                if (formatSplit[0].StartsWith("Traceback (most recent call last):", StringComparison.Ordinal))
                                {
                                    i++;
                                    while (i < formatSplit.Length && formatSplit[i] is var current && (current.Length == 0 || char.IsWhiteSpace(current[0])))
                                    {
                                        i++;
                                    }
                                }
                                for (; i < formatSplit.Length; i++)
                                {
                                    sb.AppendLine(formatSplit[i]);
                                }



                                // Split the C# stack trace into lines
                                var traceLines = wrappedEx.StackTrace?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                // Filter out the DLR and IronPython internal execution noise
                                var cleanTraceLines = traceLines?.Where(line =>
                                    !line.Contains("Microsoft.Scripting") &&
                                    !line.Contains("IronPython") &&
                                    !line.Contains("System.Dynamic.UpdateDelegates") &&
                                    !line.Contains("System.Runtime.CompilerServices.TaskAwaiter")
                                )?.ToList() ?? new();

                                // If there's anything left after filtering, it's your C# code
                                if (cleanTraceLines.Any())
                                {
                                    var sep = "\n  ";

                                    var s = "\n--- Native .NET Stack Trace (for debugging) ---"
                                        + sep
                                        + string.Join(sep, cleanTraceLines);

                                    sb.Append(s);
                                }

                                throw new PythonException(sb.ToString());
                            }

                        }
                    }
                    return false;
            }
        }

        private static bool HandleSyntaxError(SyntaxErrorException ex)
        {
            var message = new StringBuilder();
            message.AppendLine($"SyntaxError: {ex.Message}");

            var originalCodeLine = ex.GetCodeLine();

            if (!string.IsNullOrEmpty(originalCodeLine) && originalCodeLine.Length >= 10)
            {
                var trimmedCodeLine = originalCodeLine.TrimStart(' ')[8..^1];
                var trimmedColumn = Math.Max(ex.Column - (originalCodeLine.Length - trimmedCodeLine.Length + 1), 1);

                message.AppendLine($"File {searchExpressionName}, Column: {trimmedColumn}");
                message.AppendLine("\t" + trimmedCodeLine);
                message.AppendLine("\t" + new string(' ', trimmedColumn - 1) + "^");
            }
            else if (!string.IsNullOrEmpty(originalCodeLine))
            {
                message.AppendLine($"File {searchExpressionName}, Column: {ex.Column}");
                message.AppendLine("\t" + originalCodeLine);
                message.AppendLine("\t" + new string(' ', Math.Max(ex.Column - 1, 0)) + "^");
            }
            else
            {
                message.AppendLine($"File {searchExpressionName}, Column: {ex.Column}");
                message.AppendLine("<failed to recover source information>");
            }

            throw new PythonException(message.ToString(), ex);
        }




    }
}
