using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class DifficultyEvaluator : DifficultyArgumentEvaluator
        {
            public override string EvaluatorName => "Difficulty";
            public override IEnumerable<KeyValuePair<double, double>> GetDoubles(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                Utils.GetMapDifficulties(musicInfo, out var difficulties);
                foreach (var i in availableMaps)
                {
                    var musicDiff = difficulties[i-1];

                    if (musicDiff.TryParseInt(out int x))
                    {
                        yield return new(i, x);
                    }
                    else
                    {
                        yield return new(i, double.NaN);
                    }
                }
            }
        }
        internal static bool EvalDifficulty(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<DifficultyEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
