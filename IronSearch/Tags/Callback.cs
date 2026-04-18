using Il2CppAssets.Scripts.Database;
using IronSearch.Core;
using IronSearch.Utils;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        public class CallbackEvaluator : DifficultyArgumentEvaluator
        {
            public override string EvaluatorName => "Callback";
            public override bool AllowInvalid0 => false;
            public override IEnumerable<KeyValuePair<double, double>> GetPairs(MusicInfo musicInfo)
            {
                MapUtils.GetAvailableMaps(musicInfo, out var availableMaps);
                MapUtils.GetMapCallbacks(musicInfo, out var difficulties);
                foreach (var diffIndex in availableMaps)
                {
                    yield return new(diffIndex, difficulties[diffIndex - 1]);
                }
            }
        }
        internal static bool EvalCallback(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<CallbackEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}