using Il2CppAssets.Scripts.Database;
using IronSearch.Core;
using IronSearch.Patches;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        // TODO: 39-0 has 39-8 as the hidden, figure out what to do about that...
        public class UnplayedEvaluator : MapArgumentEvaluator
        {
            public override string EvaluatorName => "Unplayed";
            public override IEnumerable<KeyValuePair<double, bool>> GetPairs(MusicInfo musicInfo)
            {
                MapUtils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var diff in availableMaps)
                {
                    string s = musicInfo.uid + "_" + diff;

                    if (!SearchResults_RefreshPatch.highScores.TryGetValue(s, out var score))
                    {
                        yield return new(diff, true);
                        continue;
                    }

                    yield return new(diff, score.Clears == 0);
                }
            }
        }

        internal static bool EvalUnplayed(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<UnplayedEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
