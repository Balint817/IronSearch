using Il2CppAssets.Scripts.Database;
using IronSearch.Core;
using IronSearch.Patches;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class ClearsEvaluator : DifficultyArgumentEvaluator
        {
            public override string EvaluatorName => "Clears";
            public override bool AllowInvalid0 => false;
            public override IEnumerable<KeyValuePair<double, double>> GetPairs(MusicInfo musicInfo)
            {
                MapUtils.GetAvailableMaps(musicInfo, out var availableMaps);

                foreach (var diff in availableMaps)
                {
                    var s = musicInfo.uid + "_" + diff;

                    if (SearchResults_RefreshPatch.highScores.TryGetValue(s, out var score))
                    {
                        yield return new(diff, score.Clears);
                    }
                    else
                    {
                        yield return new(diff, 0);
                    }

                }
            }
        }
        internal static bool EvalClears(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<ClearsEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
