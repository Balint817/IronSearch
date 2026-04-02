using System.Diagnostics.CodeAnalysis;

namespace PythonExpressionManager
{
    public static class Extensions
    {
        static Extensions()
        {

            ReservedKeywords = new()
            {
                "False", "None", "True",
                "and", "as", "assert", "async", "await",
                "break", "class", "continue", "def", "del",
                "elif", "else", "except", "finally", "for",
                "from", "global", "if", "import", "in",
                "is", "lambda", "nonlocal", "not",
                "or", "pass", "raise", "return",
                "try", "while", "with", "yield"
            };
        }
        private static readonly HashSet<string> ReservedKeywords;
        public static bool IsReservedKeyword(this string keyword)
        {
            return ReservedKeywords.Contains(keyword);
        }
        private static bool IsValidFirstCharacter(char c)
        {
            return ('a' <= c && c <= 'z')
                || ('A' <= c && c <= 'Z')
                || (c == '_');
        }
        private static bool IsValidCharacter(char c)
        {
            return IsValidFirstCharacter(c)
                || ('0' <= c && c <= '9');
        }
        public static bool IsValidVariableName(this string key)
        {
            if (key is null || key.Length == 0)
            {
                return false;
            }
            return !IsReservedKeyword(key)
                && IsValidFirstCharacter(key[0])
                && key.All(IsValidCharacter)
                && !key.All(c => c == '_');
        }
        public static bool IsValidVariableName(this string key, ScriptExecutor instance)
        {
            return key.IsValidVariableName()
                && (instance.ArgumentName != key)
                && (instance.BaseDictName != key);
        }
        public static bool TryGetNativeTrace(this Exception wrappedEx, [MaybeNullWhen(false)] out string trace)
        {
            trace = null;
            if (wrappedEx is null || wrappedEx.StackTrace is null)
            {
                return false;
            }
            var traceLines = wrappedEx.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


            // Filter out the DLR and IronPython internal execution noise
            var cleanTraceLines = traceLines?.Where(line =>
                !line.Contains("Microsoft.Scripting") &&
                !line.Contains("IronPython.") &&
                !line.Contains("System.Dynamic.UpdateDelegates") &&
                !line.Contains("System.Runtime.CompilerServices.TaskAwaiter")
            )?.ToList() ?? new();

            // If there's anything left after filtering, it's your C# code
            if (!cleanTraceLines.Any())
            {
                return false;
            }

            var sep = "\n  ";

            trace = "\n--- Native .NET Stack Trace (for debugging) ---"
                + sep
                + string.Join(sep, cleanTraceLines);

            return true;
        }
        public static string? ToFilteredTrace(this Exception wrappedEx)
        {
            if (wrappedEx is null || wrappedEx.StackTrace is null)
            {
                return wrappedEx?.ToString();
            }
            var traceLines = wrappedEx.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Filter out the DLR and IronPython internal execution noise
            var cleanTraceLines = traceLines?.Where(line =>
                !line.Contains("Microsoft.Scripting") &&
                !line.Contains("IronPython.") &&
                !line.Contains("System.Dynamic.UpdateDelegates") &&
                !line.Contains("System.Runtime.CompilerServices.TaskAwaiter")
            )?.ToList() ?? new();

            var result = string.Join("\n", cleanTraceLines);

            return result;
        }
    }
}
