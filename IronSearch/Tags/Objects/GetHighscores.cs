using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Core;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetHighscores(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varKwargs, "GetHighScores", varArgs, varKwargs);
            ThrowIfNotEmpty(varArgs, "GetHighScores", varArgs, varKwargs);
            var l = new PythonList();
            for (int i = 1; i <= 5; i++)
            {
                var s = M.I.uid + "_" + i;
                if (ActiveSearch.highScores.TryGetValue(s, out var score))
                {
                    l.Add(score);
                }
                else
                {
                    l.Add(null);
                }
            }
            return l;
        }
    }
}
