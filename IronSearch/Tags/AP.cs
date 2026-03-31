using Il2CppAssets.Scripts.Database;
using IronPython.Runtime;
using IronSearch.Patches;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class APEvaluator : MapArgumentEvaluator
        {
            public override string EvaluatorName => "AP";
            public override IEnumerable<KeyValuePair<double,bool>> GetDoubles(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var diff in availableMaps)
                {
                    string s = musicInfo.uid + "_" + diff;

                    if (!RefreshPatch.highScores.TryGetValue(s, out var score))
                    {
                        yield return new(diff, false);
                        continue;

                    }

                    yield return new(diff, score.Accuracy == 1.0f);
                }
            }
        }

        internal static bool EvalAP(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<APEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
