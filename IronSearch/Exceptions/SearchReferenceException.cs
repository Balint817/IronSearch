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
            string scope;
            switch (kind)
            {
                case ReferenceKind.Local:
                    scope = "local";
                    break;
                case ReferenceKind.Global:
                    scope = "global";
                    break;
                case ReferenceKind.ThreadGlobal:
                    scope = "thread-global";
                    break;
                default:
                    scope = "<unknown kind>";
                    break;
            }
            return $"There is no {scope} variable named '{referenceName}'.";
        }

        public enum ReferenceKind
        {
            Local,
            Global,
            ThreadGlobal,
        }
    }
}
