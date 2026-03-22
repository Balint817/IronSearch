using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace PythonExpressionManager
{
    public sealed class Script: IComparable<Script>
    {
        static Script()
        {
            Engine = Python.CreateEngine(_options);
        }
        public static readonly ScriptEngine Engine;
        public static readonly string OutputFunctionName = "run";

        /// <summary>
        /// may be <see langword="null"/> if <see cref="Function"/> is a CLR method.
        /// </summary>
        internal readonly ScriptSource? Source;
        /// <summary>
        /// may be <see langword="null"/> if <see cref="Function"/> is a CLR method.
        /// </summary>
        internal readonly ScriptScope? Scope;
        public readonly dynamic Function;

        static readonly Dictionary<string, object> _options = new()
            {
                { "PrivateBinding", true }
            };

        /// <summary>
        /// Lower is higher.
        /// </summary>
        public readonly int Priority;
        private Script(int priority)
        {
            Priority = priority;
            Function = null!;
        }
        public Script(string source, int priority = (int)Priorities.CustomPython): this(priority)
        {
            Scope = Engine.CreateScope();
            Source = Engine.CreateScriptSourceFromString(source);

            //StartingSource.Execute(Scope);
            Source.Execute(Scope);

            if (!Scope.TryGetVariable(OutputFunctionName, out var outputFunction))
            {
                throw new ArgumentException($"script is missing '{OutputFunctionName}' variable", nameof(source));
            }
            if (outputFunction is null)
            {
                throw new ArgumentException($"'{OutputFunctionName}' variable was None (null)", nameof(source));
            }
            if (outputFunction is not PythonFunction)
            {
                throw new ArgumentException($"expected '{OutputFunctionName}' to be a Python function", nameof(source));
            }
            var code = ((PythonFunction)outputFunction).__code__;

            var positionalArgCount = code.co_argcount - code.co_kwonlyargcount;

            if (positionalArgCount < 2)
            {
                throw new ArgumentException($"expected '{OutputFunctionName}' to have at least 2 positional arguments (not counting *args and **kwargs)", nameof(source));
            }
            Function = outputFunction;
        }

        /// <summary>
        /// Used internally to register CLR methods.
        /// </summary>
        internal Script(dynamic function, int priority): this(priority)
        {
            Function = function;
        }
        public int CompareTo(Script? other)
        {
            if (other is null)
            {
                return -1;
            }
            return Priority.CompareTo(other.Priority);
        }
    }
}
