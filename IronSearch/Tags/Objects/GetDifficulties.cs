using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetDifficulties(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "GetDifficulties", varArgs, varKwargs);
            ThrowIfNotEmpty(varArgs, "GetDifficulties", varArgs, varKwargs);
            MapUtils.GetMapDifficulties(M.I, out var maps);
            var l = new PythonList();
            foreach (var map in maps)
            {
                l.Add(map);
            }
            return l;
        }
    }
}
