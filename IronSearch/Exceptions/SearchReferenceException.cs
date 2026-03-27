namespace IronSearch.Exceptions
{
    /// <summary>
    /// A named reference (e.g. variable) does not exist.
    /// </summary>
    public sealed class SearchReferenceException : SearchInputException
    {
        public string ReferenceName { get; }
        public ReferenceKind Kind { get; }

        public SearchReferenceException(string referenceName, ReferenceKind kind, string? parameterContext = null)
            : base(BuildMessage(referenceName, kind), parameterContext)
        {
            ReferenceName = referenceName;
            Kind = kind;
        }

        static string BuildMessage(string referenceName, ReferenceKind kind)
        {
            var scope = kind == ReferenceKind.Global ? "global" : "local";
            return $"There is no {scope} variable named '{referenceName}'.";
        }

        public enum ReferenceKind
        {
            Local,
            Global
        }
    }
}
