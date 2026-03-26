namespace IronSearch.Exceptions
{
    /// <summary>
    /// A value was not of the expected CLR type (e.g. string expected, number received).
    /// </summary>
    public sealed class SearchWrongTypeException : SearchInputException
    {
        public string ExpectedDescription { get; }
        public Type? ActualType { get; }

        public SearchWrongTypeException(
            string expectedDescription,
            Type? actualType,
            string? parameterContext = null,
            string? extraDetail = null)
            : base(
                BuildMessage(expectedDescription, actualType, extraDetail),
                parameterContext)
        {
            ExpectedDescription = expectedDescription;
            ActualType = actualType;
        }

        static string BuildMessage(string expectedDescription, Type? actualType, string? extraDetail)
        {
            var t = actualType is null ? "null" : actualType.Name;
            var msg = $"Expected {expectedDescription}, but got {t}.";
            if (!string.IsNullOrEmpty(extraDetail))
            {
                msg += " " + extraDetail;
            }
            return msg;
        }
    }
}
