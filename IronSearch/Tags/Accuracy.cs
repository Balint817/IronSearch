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
            public override bool AllowInvalid0 => false;
            public override IEnumerable<KeyValuePair<double, double>> GetPairs(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var diff in availableMaps)
                {
                    string s = musicInfo.uid + "_" + diff;

                    if (RefreshPatch.highScores.TryGetValue(s, out var score))
                    {
                        yield return new(diff, (score.AccuracyStringParsed ?? score.Accuracy)*100);
                    }
                    else
                    {
                        yield return new(diff, double.NaN);
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
