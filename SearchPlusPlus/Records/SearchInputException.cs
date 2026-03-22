using System.Runtime.Serialization;

namespace IronSearch.Records
{
    [Serializable]
    internal class SearchInputException : Exception
    {
        public SearchInputException()
        {
        }

        public SearchInputException(string? message) : base(message)
        {
        }

        public SearchInputException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SearchInputException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}