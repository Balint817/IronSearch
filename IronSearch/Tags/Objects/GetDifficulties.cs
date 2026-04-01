using IronPython.Runtime;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetDifficulties(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetDifficulties", varArgs, varKwargs);
            ThrowIfNotEmpty(varKwargs, "GetDifficulties", varArgs, varKwargs);
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
