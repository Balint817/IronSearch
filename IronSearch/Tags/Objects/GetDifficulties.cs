using IronPython.Runtime;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetDifficulties(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetDifficulties()");
            ThrowIfNotEmpty(varKwargs, "GetDifficulties()");
            Utils.GetMapDifficulties(M.I, out var maps);
            var l = new PythonList();
            foreach (var map in maps)
            {
                l.Add(map);
            }
            return l;
        }
    }
}
