using Il2CppAssets.Scripts.Database;
using IronSearch.Core;
using IronSearch.Loaders;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        private class LengthEvaluator : TimeRangeArgumentEvaluator
        {
            public override string EvaluatorName => "Length";
            public override IEnumerable<double> GetDoubles(MusicInfo musicInfo)
            {
                var length = ChartDataLoader.GetMusicLength(musicInfo);
                if (length is not { } ts)
                {
                    return Array.Empty<double>();
                }
                return new[] { ts.TotalSeconds };
            }
        }
        internal static bool EvalLength(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<LengthEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
