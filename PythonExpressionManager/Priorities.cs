namespace PythonExpressionManager
{
    /// <summary>
    /// You don't need to actually use these values, it's more of a suggestion based on how I'll be using this.
    /// </summary>
    public enum Priorities
    {
        BuiltIn = -2,
        CustomCLR = -1,
        CustomPython = 0,
        UserScript = 1,
        Expression = 2,
        Alias = 3,
    }
}
