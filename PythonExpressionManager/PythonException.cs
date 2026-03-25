using System.Runtime.Serialization;

namespace PythonExpressionManager
{
    [Serializable]
    public class PythonException : Exception
    {
        public PythonException()
        {
        }

        public PythonException(string? message) : base(message)
        {
        }

        public PythonException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PythonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public override string ToString()
        {
            return Message;
        }
    }
}