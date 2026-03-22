namespace PythonExpressionManager
{
    public static class Extensions
    {
        static Extensions()
        {
            //var list = Script.Engine.Execute("import keyword\nkeyword.kwlist");
            //ReservedKeywords = list.ToHashSet<string>();

            ReservedKeywords = new()
            {
                "and", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except",
                "exec", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "not", "or",
                "pass", "print", "raise", "return", "try", "while", "with", "yield",
                
                //"bool" // used to be reserved because CompiledScript used it, but was abandoned in favor of "not not"
            };
        }
        static readonly HashSet<string> ReservedKeywords;
        public static bool IsReservedKeyword(this string keyword)
        {
            return ReservedKeywords.Contains(keyword);
        }
        static bool IsValidFirstCharacter(char c)
        {
            return ('a' <= c && c <= 'z')
                || ('A' <= c && c <= 'Z')
                || (c == '_');
        }
        static bool IsValidCharacter(char c)
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
    }
}
