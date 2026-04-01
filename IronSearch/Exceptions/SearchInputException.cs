namespace IronSearch.Exceptions
{
    /// <summary>
    /// Base type for user-facing search/tag input errors. Prefer derived types when they fit;
    /// use this when a simple message is enough.
    /// </summary>
    public class SearchInputException : Exception
    {
        /// <summary>
        /// Optional context for what part of the search failed (e.g. tag name, built-in name, parameter name).
        /// </summary>
        public string? ParameterContext { get; }

        public SearchInputException(string message, string? parameterContext = null, Exception? innerException = null)
            : base(message, innerException)
        {
            ParameterContext = parameterContext;
        }

        public override string ToString()
        {
            if (ParameterContext is not null)
            {
                return $"Error in {ParameterContext}: " + Message;
            }
            return "Error: " + Message;
        }
    }
}
