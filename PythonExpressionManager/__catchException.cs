using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using System.Runtime.Serialization;

namespace PythonExpressionManager
{
    [Serializable]
    internal class __catchException : Exception
    {
        public readonly PythonType _errorType;
        public readonly string _errorName;
        public readonly string _errorMessage;
        public readonly object _originalException;

        public __catchException(PythonType errorType, string errorName, string errorMessage, object originalException)
        {
            this._errorType = errorType;
            this._errorName = errorName;
            this._errorMessage = errorMessage;
            this._originalException = originalException;
        }
        public override string ToString()
        {
            return $"{_errorName}: {_errorMessage}";
        }
    }
}