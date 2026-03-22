using IronPython.Runtime;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetHighscores(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs);
            ThrowIfNotEmpty(varKwargs);
            var l = new PythonList();
            foreach (var item in RefreshPatch.highScores)
            {
                l.Add(item);
            }
            return l;
        }
    }
}
