using Il2CppAssets.Scripts.Database;

namespace IronSearch.Tags
{
    internal partial class BuiltIns
    {
        public class ModifiedEvaluator : TimeRangeArgumentEvaluator
        {
            public override string EvaluatorName => "Modified";
            public override IEnumerable<double> GetDoubles(MusicInfo musicInfo)
            {
                var modified = GetModified(musicInfo);
                if (modified is not { } modifiedDate)
                {
                    yield break;
                }
                yield return DateTime.Now.Subtract(modifiedDate).TotalSeconds;
            }
        }
        internal static bool EvalModified(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
        {
            return ManagedSingleton<ModifiedEvaluator>.Instance.Evaluate(M, varArgs, varKwargs);
        }
    }
}
