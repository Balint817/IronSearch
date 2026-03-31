using IronPython.Runtime;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        internal static dynamic EvalGetHighscores(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            ThrowIfNotEmpty(varArgs, "GetHighScores()");
            ThrowIfNotEmpty(varKwargs, "GetHighScores()");
            var l = new PythonList();
            for (int i = 1; i <= 5; i++)
            {
                var s = M.I.uid + "_" + i;
                if (RefreshPatch.highScores.TryGetValue(s, out var score))
                {
                    l.Add(score);
                }
            }
            return l;
        }
    }
}
