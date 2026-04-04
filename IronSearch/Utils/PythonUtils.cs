using IronPython.Runtime;
using IronSearch.Records;
using MelonLoader;
using PythonExpressionManager;

namespace IronSearch.Utils
{
    public static class PythonUtils
    {
        // this is a fucking mess and i wanna kms
        internal static void PrintSearchError(this SearchResponse response, string baseMsg = "The current search resulted in an error. (Code: {0})")
        {
            MelonLogger.Msg(ConsoleColor.Red, string.Format(baseMsg, response.Code));

            if (response.Message != null)
            {
                MelonLogger.Msg(ConsoleColor.Magenta, response.Message);
            }
            if (response.Exception != null)
            {
                switch (response.Exception)
                {
                    case PythonException pe:
                        MelonLogger.Msg(ConsoleColor.Red, response.Exception.Message);
                        break;
                    default:
                        MelonLogger.Msg(ConsoleColor.Red, response.Exception);
                        break;
                }
            }
        }

        public static bool IsCallable(dynamic obj)
        {
            try
            {
                if (obj is Delegate)
                {
                    return true;
                }
                var engine = IronPython.Hosting.Python.CreateEngine();
                return engine.Operations.IsCallable(obj);
            }
            catch
            {
                return false;
            }
        }
        public static int GetPythonArgCount(dynamic func)
        {
            if (func is PythonFunction pyFunc)
            {
                return pyFunc.__code__.co_argcount;
            }
            return -1;
        }
    }
}
