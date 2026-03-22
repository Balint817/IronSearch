namespace PythonExpressionManager
{
    public class ConsoleLogger : ILogger
    {
        public void LogDebug(object message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"DEBUG: {message?.ToString()}");
            Console.ResetColor();
        }
        public void LogDebug(object message) => LogDebug(message, Console.ForegroundColor);
        public void LogInfo(object message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"INFO: {message?.ToString()}");
            Console.ResetColor();
        }
        public void LogInfo(object message) => LogInfo(message, Console.ForegroundColor);

        public void LogWarning(object message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"WARNING: {message?.ToString()}");
            Console.ResetColor();
        }
        public void LogWarning(object message) => LogWarning(message, ConsoleColor.Yellow);

        public void LogError(object message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"ERROR: {message?.ToString()}");
            Console.ResetColor();
        }
        public void LogError(object message) => LogError(message, ConsoleColor.Red);
    }
}
