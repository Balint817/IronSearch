using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class DifficultyEvaluator : DifficultyArgumentEvaluator
        {
            public override string EvaluatorName => "Difficulty";
            public override bool AllowInvalid0 => true;
            public override IEnumerable<KeyValuePair<double, double>> GetPairs(MusicInfo musicInfo)
            {
                if (!Utils.GetMapDifficulties(musicInfo, out var difficulties))
                {
                    yield break;
                }

                for (int i = 0; i < 5; i++)
                {
                    var musicDiff = difficulties[i];
                    if (musicDiff is null)
                    {
                        continue;
                    }


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
