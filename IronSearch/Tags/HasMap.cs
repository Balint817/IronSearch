using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class HasMapEvaluator : RangeArgumentEvaluator
        {
            public override string EvaluatorName => "Map";
            public override IEnumerable<double> GetDoubles(MusicInfo musicInfo)
            {
                Utils.GetAvailableMaps(musicInfo, out var availableMaps);
                foreach (var item in availableMaps)
                {
                    yield return item;
                }
            }
        }
        internal static bool EvalHasMap(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<HasMapEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
