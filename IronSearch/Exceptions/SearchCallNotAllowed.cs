namespace IronSearch.Exceptions
{
    /// <summary>
    /// Thrown when a comparer/object is called a second time instead of being passed as an argument.
    /// </summary>
    public sealed class SearchCallNotAllowed : SearchInputException
    {
        public SearchCallNotAllowed(string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            : base(BuildMessage(parameterContext), parameterContext, varArgs, varKwargs)
        {

        }
        private static string BuildMessage(string parameterContext)
        {
            var s = $"You're not supposed to call this twice, like {parameterContext}, pass it as an argument instead!";
            return s;
        }
    }
}
