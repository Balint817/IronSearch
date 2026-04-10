using HarmonyLib;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronSearch.Records;
using MelonLoader;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using PythonExpressionManager;
using System.Diagnostics.CodeAnalysis;

namespace IronSearch.Utils
{
    class NameCallWalker : PythonWalker
    {
        public NameCallWalker(List<string> varList, List<string> callList)
        {
            VarList = varList;
            CallList = callList;
        }
        public List<string> VarList { get; }
        public List<string> CallList { get; }

        public override bool Walk(NameExpression node)
        {
            VarList.Add(node.Name);
            return base.Walk(node);
        }

        public override bool Walk(CallExpression node)
        {
            if (node.Target is NameExpression name)
            {
                CallList.Add(name.Name);
            }

            return base.Walk(node);
        }
    }
    public static class PythonUtils
    {
        // this is a fucking mess and i wanna kms
        internal static void PrintSearchError(this SearchResponse response, string baseMsg = "The current search resulted in an error. (Code: {0})")
        {
            MelonLogger.Msg(ConsoleColor.Red, string.Format(baseMsg, response.Code));

            if (response.Message != null)
            {
                MelonLogger.Msg(ConsoleColor.Magenta, response.Message);
            }
            if (response.Exception != null)
            {
                switch (response.Exception)
                {
                    case PythonException pe:
                        MelonLogger.Msg(ConsoleColor.Red, response.Exception.Message);
                        break;
                    default:
                        MelonLogger.Msg(ConsoleColor.Red, response.Exception);
                        break;
                }
            }
        }

        public static bool GetPythonNamesFromAST(ScriptEngine engine, string code, [MaybeNullWhen(false)]out List<string> varList, [MaybeNullWhen(false)] out List<string> callList)
        {
            varList = null;
            callList = null;
            try
            {
                var context = HostingHelpers.GetLanguageContext(engine);
                var sourceUnit = context.CreateSnippet(code, SourceCodeKind.File);

                var options = (PythonCompilerOptions)engine.GetCompilerOptions();
                var compilerContext = new CompilerContext(sourceUnit, options, ErrorSink.Default);

                var parser = Parser.CreateParser(compilerContext, new());

                PythonAst ast = parser.ParseFile(true);

                varList = new List<string>();
                callList = new List<string>();
                ast.Walk(new NameCallWalker(varList, callList));
                return true;
            }
            catch (Exception)
            {
                varList = null;
                callList = null;
                return false;
            }
        }

        public static bool IsCallable(dynamic obj)
        {
            try
            {
                if (obj is Delegate)
                {
                    return true;
                }
                var engine = IronPython.Hosting.Python.CreateEngine();
                return engine.Operations.IsCallable(obj);
            }
            catch
            {
                return false;
            }
        }
        public static int GetPythonArgCount(dynamic func)
        {
            if (func is PythonFunction pyFunc)
            {
                return pyFunc.__code__.co_argcount;
            }
            return -1;
        }
    }
}
