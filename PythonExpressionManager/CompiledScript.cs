using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Scripting.Hosting;

namespace PythonExpressionManager
{
    public sealed class CompiledScript
    {
        const string tagDict = "___";
        const string args = "____";
        const string kwargs = "_____";
        const string f1 = "______";
        const string f2 = "_______";
        internal dynamic Function;
        //internal ReadOnlyDictionary<string, dynamic> Scripts;
        internal CompiledScript(string body, ScriptExecutor instance)
        {
            var Scripts = new ReadOnlyDictionary<string, dynamic>(instance.RegisteredKeys.ToDictionary(x => x.Key, x => x.Value.Function));


            var scriptBuilder = new StringBuilder(
                $"def {f1}({instance.ArgumentName}, **{instance.BaseDictName}):\n"
                );

            scriptBuilder.AppendLine($"\t{tagDict} = {{}}");

            foreach (var item in Scripts)
            {
                scriptBuilder.AppendLine($"\t{tagDict}['{item.Key}'] = {item.Key} = lambda *{args}, **{kwargs}: {instance.BaseDictName}['{item.Key}']({instance.ArgumentName}, {instance.BaseDictName}, *{args}, **{kwargs})");

            }


            scriptBuilder.AppendLine($"\treturn ({body.Replace("\n", "\\n")})");
            scriptBuilder.AppendLine($"\treturn");

            //scriptBuilder.AppendLine("\ttry:");
            //scriptBuilder.AppendLine($"\t\treturn ({body.Replace("\n", "\\n")})");
            //scriptBuilder.AppendLine($"\t\treturn");
            //scriptBuilder.AppendLine("\texcept SystemExit:");
            //scriptBuilder.AppendLine("\t\traise");
            //scriptBuilder.AppendLine($"{f2} = lambda {instance.BaseDictName}: (lambda {instance.ArgumentName}: {f1}({instance.ArgumentName}, **{instance.BaseDictName}))");

            var script = scriptBuilder.ToString();
            var source = Script.Engine.CreateScriptSourceFromString(script);
            var scope = Script.Engine.CreateScope();
            try
            {
                source.Execute(scope);
                var functionWrapper = scope.GetVariable(f2);
                Function = functionWrapper(Scripts);
            }
            catch (Exception ex)
            {
                try
                {
                    var engine = scope.Engine;
                    var eo = engine.GetService<ExceptionOperations>();
                    string error = eo.FormatException(ex);
                    throw new PythonException(error);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
        }
    }
}
