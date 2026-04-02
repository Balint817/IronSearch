namespace IronSearch.Exceptions
{
    /// <summary>
    /// A named reference (e.g. variable) does not exist.
    /// </summary>
    public sealed class SearchReferenceException : SearchInputException
    {
        public string ReferenceName { get; }
        public ReferenceKind Kind { get; }

        public SearchReferenceException(string referenceName, ReferenceKind kind, string parameterContext, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            : base(BuildMessage(referenceName, kind), parameterContext, varArgs, varKwargs)
        {
            ReferenceName = referenceName;
            Kind = kind;
        }

        private static string BuildMessage(string referenceName, ReferenceKind kind)
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
