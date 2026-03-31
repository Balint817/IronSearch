using Il2CppAssets.Scripts.Database;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        // TODO: 39-0 has 39-8 as the hidden, figure out what to do about that...
        public class UnplayedEvaluator : MapArgumentEvaluator
        {
            public override string EvaluatorName => "Unplayed";
            public override IEnumerable<KeyValuePair<double, bool>> GetDoubles(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var diff in availableMaps)
                {
                    string s = musicInfo.uid + "_" + diff;

                    yield return new(diff, !RefreshPatch.highScores.ContainsKey(s));
                }
            }
        }

        internal static bool EvalUnplayed(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<UnplayedEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
