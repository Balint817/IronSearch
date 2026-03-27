namespace IronSearch.Exceptions
{
    /// <summary>
    /// Input could not be parsed into the expected form (range, multi-range, time, etc.).
    /// </summary>
    public sealed class SearchParseException : SearchInputException
    {
        /// <summary>What the user supplied (may be truncated for very long input).</summary>
        public string? RawInput { get; }

        /// <summary>Why parsing failed, when known.</summary>
        public string? FailureDetail { get; }

        /// <summary>What format was expected (human-readable).</summary>
        public string? ExpectedDescription { get; }

        public SearchParseException(
            string message,
            string? rawInput = null,
            string? failureDetail = null,
            string? expectedDescription = null,
            string? parameterContext = null,
            Exception? innerException = null)
            : base(message, parameterContext, innerException)
        {
            RawInput = rawInput;
            FailureDetail = failureDetail;
            ExpectedDescription = expectedDescription;
        }

        /// <summary>
        /// Builds a detailed message when <see cref="Utils.ParseRange"/> fails.
        /// </summary>
        public static SearchParseException ForRange(
            string expression,
            string? parameterContext,
            string? expectedDescription,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity)
        {
            var result = Utils.ParseRange(expression, out _, min, max, out var reason);
            if (result == true)
            {
                throw new InvalidOperationException("ForRange was called but the expression parsed successfully.");
            }

            reason ??= "The range string was invalid.";
            var msg = $"Could not parse \"{expression}\" as a range.\n{reason}";
            if (!string.IsNullOrEmpty(expectedDescription))
            {
                msg += $"\nExpected: {expectedDescription}.";
            }

            return new SearchParseException(msg, expression, reason, expectedDescription, parameterContext);
        }

        /// <summary>
        /// Builds a detailed message when <see cref="Utils.ParseMultiRange"/> fails.
        /// </summary>
        public static SearchParseException ForMultiRange(
            string expression,
            string? parameterContext,
            string? expectedDescription,
            double min = double.NegativeInfinity,
            double max = double.PositiveInfinity)
        {
            var result = Utils.ParseMultiRange(expression, out _, min, max, out var reason);
            if (result == true)
            {
                throw new InvalidOperationException("ForMultiRange was called but the expression parsed successfully.");
            }

            reason ??= "The multi-range string was invalid.";
            var msg = $"Could not parse \"{expression}\" as a multi-range.\n{reason}";
            if (!string.IsNullOrEmpty(expectedDescription))
            {
                msg += $"\nExpected: {expectedDescription}.";
            }

            return new SearchParseException(msg, expression, reason, expectedDescription, parameterContext);
        }
    }
}
