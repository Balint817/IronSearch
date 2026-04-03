using Il2CppAssets.Scripts.Database;
using IronSearch.Core;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class OldEvaluator : RangeArgumentEvaluator
        {
            public override string EvaluatorName => "Old";
            public override IEnumerable<double> GetDoubles(MusicInfo musicInfo)
            {
                InitNewIfNeeded();
                var idx = sortedByLastModified!.IndexOf(musicInfo.uid);
                if (idx == -1)
                {
                    yield break;
                }
                idx = sortedByLastModified.Count - 1 - idx;
                yield return (double)idx;
            }
        }
        internal static bool EvalOld(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<OldEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
