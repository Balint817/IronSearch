using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

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


            scriptBuilder.AppendLine($"\ttry:");
            scriptBuilder.AppendLine($"\t\treturn ({body.Replace("\n", "\\n")})");
            scriptBuilder.AppendLine($"\texcept Exception as ex:");
            scriptBuilder.AppendLine($"\t\timport sys");
            scriptBuilder.AppendLine($"\t\timport clr");
            scriptBuilder.AppendLine($"\t\tclr.AddReference('{nameof(PythonExpressionManager)}')");
            scriptBuilder.AppendLine($"\t\tfrom {nameof(PythonExpressionManager)} import __internalException");
            scriptBuilder.AppendLine($"\t\texc_type, exc_value, _ = sys.exc_info()");
            scriptBuilder.AppendLine($"\t\traise __internalException(exc_type, exc_type.__name__, str(exc_value), ex)");

            scriptBuilder.AppendLine($"\treturn");

            scriptBuilder.AppendLine($"{f2} = lambda {instance.BaseDictName}: (lambda {instance.ArgumentName}: {f1}({instance.ArgumentName}, **{instance.BaseDictName}))");

            var script = scriptBuilder.ToString();
            var source = Script.Engine.CreateScriptSourceFromString(script, searchExpressionName, Microsoft.Scripting.SourceCodeKind.File);
            var scope = Script.Engine.CreateScope();
            try
            {
                source.Execute(scope);
                var functionWrapper = scope.GetVariable(f2);
                Function = functionWrapper(Scripts);
            }
            catch (Exception ex)
            {
                ConvertException(ex);
                throw;
            }
        }

        public static void ConvertException(Exception wrappedEx)
        {
            switch (wrappedEx)
            {
                case Microsoft.Scripting.SyntaxErrorException ex:
                    {
                        var message = new StringBuilder();
                        message.AppendLine(ex.Message);

                        var originalCodeLine = ex.GetCodeLine();

                        if (!string.IsNullOrEmpty(originalCodeLine) || originalCodeLine.Length < 10)
                        {
                            var trimmedCodeLine = originalCodeLine.TrimStart(' ')[8..^1];
                            var trimmedColumn = Math.Max(ex.Column - (originalCodeLine.Length - trimmedCodeLine.Length + 1), 1);

                            message.AppendLine($"File {searchExpressionName}, Column: {trimmedColumn}");
                            message.AppendLine("\t" + trimmedCodeLine);
                            message.AppendLine("\t" + new string(' ', trimmedColumn - 1) + "^");
                        }
                        else
                        {
                            message.AppendLine($"File {searchExpressionName}, Column: {ex.Column}");
                            message.AppendLine("<failed to recover information>");
                        }
                        throw new PythonException(message.ToString(), ex);
                    }
                case IronPython.Runtime.UnboundNameException ex:
                    throw new PythonException("NameError: " + ex.Message);
                case __internalException ex:
					if (ex._originalException is System.Exception sysEx)
					{
						throw sysEx;
					}
					if (PythonOps.IsPythonType(ex._errorType))
					{
						throw new PythonException(ex.ToString());
					}
					throw new Exception("an unknown exception occured", ex);
            }

            throw wrappedEx;
        }
    }
}
