using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using PythonExpressionManager;

namespace IronSearch.UI
{
    public class ScriptMelonLogger : ILogger
    {
        public void LogDebug(object message, ConsoleColor color)
        {
            //MelonLogger.Msg(color, $"DEBUG: {message?.ToString()}");
        }
        public void LogDebug(object message) => LogDebug(message, Console.ForegroundColor);
        public void LogInfo(object message, ConsoleColor color)
        {
            MelonLogger.Msg(color, $"INFO: {message?.ToString()}");
        }
        public void LogInfo(object message) => LogInfo(message, Console.ForegroundColor);

        public void LogWarning(object message, ConsoleColor color)
        {
            MelonLogger.Msg(color, $"WARNING: {message?.ToString()}");
        }
        public void LogWarning(object message) => LogWarning(message, ConsoleColor.Yellow);

        public void LogError(object message, ConsoleColor color)
        {
            MelonLogger.Msg(color, $"ERROR: {message?.ToString()}");
        }
        public void LogError(object message) => LogError(message, ConsoleColor.Red);
    }
}
