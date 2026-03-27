namespace IronSearch.Records
{
    public class SearchResponse
    {
        public static bool IsErrorCode(Type type)
        {
            switch (type)
            {
                case Type.SearchPassed:
                case Type.SearchFailed:
                    return false;
                default:
                    return true;
            }
        }
        public enum Type
        {
            TimeoutError = -3,
            ParserError = -2,
            RuntimeError = -1,
            SearchPassed = 0,
            SearchFailed = 1,
        }

        public readonly Exception? Exception;
        public readonly string? Message;
        public readonly Type Code;

        public SearchResponse(Type code = Type.SearchFailed)
        {
            Code = code;
        }
        public SearchResponse(string message, Type code = Type.SearchFailed)
        {
            Message = message;
            Code = code;
        }
        public SearchResponse(Exception ex, Type code = Type.SearchFailed)
        {
            Exception = ex;
            Code = code;
        }
        public SearchResponse(string message, Exception ex, Type code = Type.SearchFailed)
        {
            Message = message;
            Exception = ex;
            Code = code;
        }
    }

}
