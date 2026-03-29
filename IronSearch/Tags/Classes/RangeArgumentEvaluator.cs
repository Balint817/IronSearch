using Il2CppAssets.Scripts.Database;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal abstract class RangeArgumentEvaluator: Evaluator
        {
            public abstract IEnumerable<double> GetDoubles(MusicInfo musicInfo);
            public override bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            {
                ThrowIfNotEmpty(varKwargs, EvaluatorNameCalled);
                ThrowIfNotMatching(varArgs, 1, EvaluatorNameCalled);

                MultiRange mr = MultiRangeArgumentParser.GetMultiRange(varArgs[0], EvaluatorNameCalled);

                return GetDoubles(M.I).Any(value => mr.Contains(value));
            }
        }
    }
}
