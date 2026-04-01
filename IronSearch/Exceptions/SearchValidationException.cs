namespace IronSearch.Exceptions
{
    /// <summary>
    /// A value failed a validity check (empty string, out of range, disallowed token, etc.).
    /// </summary>
    public sealed class SearchValidationException : SearchInputException
    {
        public SearchValidationException(string message, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs, Exception? innerException = null)
            : base(message, parameterContext, varArgs, varKwargs, innerException)
        {
        }
    }
}
