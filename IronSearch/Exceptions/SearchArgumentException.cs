using Range = IronSearch.Records.Range;

namespace IronSearch.Exceptions
{
    /// <summary>
    /// Wrong number of arguments, unexpected keyword arguments, or missing required arguments.
    /// </summary>
    public sealed class SearchArgumentException : SearchInputException
    {
        public int? ExpectedCount { get; }
        public int? ActualCount { get; }
        public IReadOnlyList<string>? UnexpectedKeywordNames { get; }

        public SearchArgumentException(
            string message,
            string parameterContext,
            dynamic[] varArgs,
            Dictionary<string, dynamic> varKwargs,
            int? expectedCount = null,
            int? actualCount = null,
            IReadOnlyList<string>? unexpectedKeywordNames = null,
            Exception? innerException = null)
            : base(message, parameterContext, varArgs, varKwargs, innerException)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
            UnexpectedKeywordNames = unexpectedKeywordNames;
        }

        public static SearchArgumentException UnexpectedKeywords(IEnumerable<string> keywordNames, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            var names = keywordNames.ToList();
            var joined = string.Join(", ", names.Select(x => $"'{x}'"));
            return new SearchArgumentException(
                $"Unexpected keyword argument(s): {joined}.",
                parameterContext, varArgs, varKwargs,
                unexpectedKeywordNames: names);
        }

        public static SearchArgumentException UnexpectedPositionalArguments(string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return new SearchArgumentException("Unexpected positional argument(s).", parameterContext, varArgs, varKwargs);
        }

        public static SearchArgumentException ExpectedAtLeastOnePositional(string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return new SearchArgumentException("Expected at least one positional argument.", parameterContext, varArgs, varKwargs);
        }

        public static SearchArgumentException ExpectedAtLeastNPositional(int n, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return new SearchArgumentException($"Expected at least {n} positional argument(s).", parameterContext, varArgs, varKwargs);
        }

        public static SearchArgumentException ArgumentCountNotInRange(Range expectedRange, int actualCount, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return new SearchArgumentException(
                $"Expected {expectedRange} argument(s), but got {actualCount}.",
                parameterContext, varArgs, varKwargs,
                expectedCount: null,
                actualCount: actualCount);
        }

        public static SearchArgumentException ArgumentCountMismatch(int expectedCount, int actualCount, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return new SearchArgumentException(
                $"Expected {expectedCount} argument(s), but got {actualCount}.",
                parameterContext, varArgs, varKwargs,
                expectedCount,
                actualCount);
        }
    }
}
