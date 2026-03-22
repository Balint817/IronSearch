namespace PythonExpressionManager
{
    public interface ILogger
    {
        void LogDebug(object message);
        void LogDebug(object message, ConsoleColor color);
        void LogInfo(object message);
        void LogInfo(object message, ConsoleColor color);
        void LogWarning(object message);
        void LogWarning(object message, ConsoleColor color);
        void LogError(object message);
        void LogError(object message, ConsoleColor color);
    }
}
