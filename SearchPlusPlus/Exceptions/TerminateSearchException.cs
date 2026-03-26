using System.Runtime.Serialization;

namespace IronSearch.Exceptions
{
    [Serializable]
    internal class TerminateSearchException : Exception
    {
        internal readonly bool IsTrue;

        public TerminateSearchException()
        {
        }

        public TerminateSearchException(bool b)
        {
            this.IsTrue = b;
        }

        public TerminateSearchException(string? message) : base(message)
        {
        }

        public TerminateSearchException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TerminateSearchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}