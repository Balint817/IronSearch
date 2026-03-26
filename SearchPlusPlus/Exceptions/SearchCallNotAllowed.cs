namespace IronSearch.Exceptions
{
    /// <summary>
    /// A named reference (e.g. variable) does not exist.
    /// </summary>
    public sealed class SearchCallNotAllowed : SearchInputException
    {
        public SearchCallNotAllowed(string? parameterContext = null)
            : base(BuildMessage(parameterContext), parameterContext)
        {

        }
        static string BuildMessage(string? parameterContext)
        {
            var s = "You're not supposed to call this twice, ";
            if (parameterContext != null)
            {
                s += $"like {parameterContext}, ";
            }
            s += "pass it as an argument instead!";
            return s;
        }
    }
}
