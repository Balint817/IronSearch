using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        public class AccuracyEvaluator : DifficultyArgumentEvaluator
        {
            public override string EvaluatorName => "Accuracy";
            public override IEnumerable<KeyValuePair<double, double>> GetDoubles(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var diff in availableMaps)
                {
                    string s = musicInfo.uid + "_" + diff;


                    var scores = RefreshPatch.highScores.Where(x => x.Uid == s).ToArray();
                    if (scores.Length == 0)
                    {
                        yield return new(diff, double.NaN);
                        continue;
                    }
                    foreach (var item in scores)
                    {
                        yield return new(diff, item.Accuracy);
                    }
                }
            }
        }
        internal static bool EvalAccuracy(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<AccuracyEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
