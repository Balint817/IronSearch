using Il2CppAssets.Scripts.Database;
using IronSearch.Records;

namespace IronSearch.Tags
{
    internal static partial class BuiltIns
    {
        internal abstract class TimeRangeArgumentEvaluator : Evaluator
        {
            public abstract IEnumerable<double> GetDoubles(MusicInfo musicInfo);
            public override bool Evaluate(SearchArgument M, dynamic[] varArgs, Dictionary<string, dynamic> varKwargs)
            {
                ThrowIfNotEmpty(varKwargs, EvaluatorName, varArgs, varKwargs);
                ThrowIfEmpty(varArgs, EvaluatorName, varArgs, varKwargs);

                MultiRange mr = MultiRangeArgumentParser.GetMultiRange(varArgs[0], EvaluatorName, varArgs, varKwargs, true);

                return GetDoubles(M.I).Any(value => mr.Contains(value));
            }
        }
    }
}
