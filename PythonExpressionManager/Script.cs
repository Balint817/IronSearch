using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace PythonExpressionManager
{
    public sealed class Script: IComparable<Script>, IDisposable
    {
        public static readonly string OutputFunctionName = "run";

        /// <summary>
        /// may be <see langword="null"/> if <see cref="Function"/> is a CLR method.
        /// </summary>
        internal ScriptSource? Source { get; private set; }
        /// <summary>
        /// may be <see langword="null"/> if <see cref="Function"/> is a CLR method.
        /// </summary>
        internal ScriptScope? Scope { get; private set; }
        public dynamic Function { get; private set; }

        /// <summary>
        /// Lower is higher.
        /// </summary>
        public readonly int Priority;
        private bool disposedValue;

        private Script(int priority)
        {
            Priority = priority;
            Function = null!;
        }
        public Script(ScriptEngine engine, string source, int priority = (int)Priorities.CustomPython): this(priority)
        {
            Scope = engine.CreateScope();
            Source = engine.CreateScriptSourceFromString(source);

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

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source = null;
                    Scope = null;
                    Function = null!;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Script()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
