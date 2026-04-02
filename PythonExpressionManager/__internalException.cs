using IronPython.Runtime.Types;

namespace PythonExpressionManager
{
    [Serializable]
    internal class __internalException : Exception
    {
        public readonly PythonType _errorType;
        public readonly string _errorName;
        public readonly string _errorMessage;
        public readonly object _originalException;
        public __internalException(PythonType errorType, string errorName, string errorMessage, object originalException) : base(errorMessage, originalException as System.Exception)
        {
            _errorType = errorType;
            _errorName = errorName;
            _errorMessage = errorMessage;
            _originalException = originalException;
        }
        public override string ToString()
        {
            return $"{_errorName}: {_errorMessage}";
        }
    }
}