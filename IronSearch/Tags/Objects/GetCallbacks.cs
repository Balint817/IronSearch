using IronPython.Runtime;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetCallbacks(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetCallbacks", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "GetCallbacks", varArgs, varKwargs);
            MapUtils.GetMapCallbacks(M.I, out var maps);
            var l = new PythonList();
            foreach (var map in maps)
            {
                l.Add(map);
            }
            return l;
        }
    }
}
