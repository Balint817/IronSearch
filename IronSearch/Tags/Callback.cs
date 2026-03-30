using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {

        public class CallbackEvaluator : DifficultyArgumentEvaluator
        {
            public override string EvaluatorName => "Callback";
            public override IEnumerable<KeyValuePair<double, double>> GetDoubles(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                Utils.GetMapCallbacks(musicInfo, out var difficulties);
                foreach (var i in availableMaps)
                {
                    yield return new(i, difficulties[i-1]);
                }
            }
        }
        internal static bool EvalCallback(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<CallbackEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}